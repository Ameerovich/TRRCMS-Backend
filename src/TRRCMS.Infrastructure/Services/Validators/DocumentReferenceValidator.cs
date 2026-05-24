using System.Diagnostics;
using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Entities.Staging;

namespace TRRCMS.Infrastructure.Services.Validators;

/// <summary>
/// Level 2: Document Reference Validator.
///
/// BuildingDocument and IdentificationDocument are committed by resolving their parent
/// FK (BuildingDocument.OriginalBuildingId → Building, IdentificationDocument.OriginalPersonId
/// → Person) through the commit-time id-map. That map is only populated for parents that are
/// present in the same package. If the parent is missing — because it wasn't sent, or because
/// the mobile app stored a non-UUID value that was read as a random GUID — the commit silently
/// skips the document and its already-extracted file is orphaned on disk.
///
/// This validator catches that case up front:
///   - BuildingDocument.OriginalBuildingId must resolve to a StagingBuilding in the batch (error)
///   - IdentificationDocument.OriginalPersonId must resolve to a StagingPerson in the batch (error)
///   - The document's file must have been extracted to server storage (warning if missing — the
///     metadata still commits, but the file won't be downloadable until re-sent with a BLOB)
/// </summary>
public class DocumentReferenceValidator : IStagingValidator
{
    public string Name => "DocumentReferenceValidator";
    public int Level => 2;

    private readonly IStagingRepository<StagingBuildingDocument> _buildingDocRepo;
    private readonly IStagingRepository<StagingIdentificationDocument> _idDocRepo;
    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPerson> _personRepo;
    private readonly IFileStorageService _fileStorageService;

    public DocumentReferenceValidator(
        IStagingRepository<StagingBuildingDocument> buildingDocRepo,
        IStagingRepository<StagingIdentificationDocument> idDocRepo,
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPerson> personRepo,
        IFileStorageService fileStorageService)
    {
        _buildingDocRepo = buildingDocRepo;
        _idDocRepo = idDocRepo;
        _buildingRepo = buildingRepo;
        _personRepo = personRepo;
        _fileStorageService = fileStorageService;
    }

    public async Task<ValidatorResult> ValidateAsync(
        Guid importPackageId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        int totalErrors = 0, totalWarnings = 0, totalChecked = 0;

        var buildings = await _buildingRepo.GetByPackageIdAsync(importPackageId, ct);
        var persons = await _personRepo.GetByPackageIdAsync(importPackageId, ct);
        var buildingDocs = await _buildingDocRepo.GetByPackageIdAsync(importPackageId, ct);
        var idDocs = await _idDocRepo.GetByPackageIdAsync(importPackageId, ct);

        var buildingIds = buildings.Select(b => b.OriginalEntityId).ToHashSet();
        var personIds = persons.Select(p => p.OriginalEntityId).ToHashSet();

        // BuildingDocument → Building
        foreach (var doc in buildingDocs)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(doc.OriginalFileName))
                errors.Add("OriginalFileName is required");

            if (doc.OriginalBuildingId == Guid.Empty || !buildingIds.Contains(doc.OriginalBuildingId))
                errors.Add($"BuildingDocument references Building {doc.OriginalBuildingId} which does not exist in the package — " +
                           "the file would be dropped at commit. Ensure building_documents.building_id is the building's id (UUID).");

            await CheckFileAsync(doc.FilePath, doc.OriginalFileName, warnings);

            if (ApplyResults(doc, errors, warnings))
            {
                totalErrors += errors.Count;
                totalWarnings += warnings.Count;
            }
            totalChecked++;
        }

        // IdentificationDocument → Person
        foreach (var doc in idDocs)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (doc.OriginalPersonId == Guid.Empty || !personIds.Contains(doc.OriginalPersonId))
                errors.Add($"IdentificationDocument references Person {doc.OriginalPersonId} which does not exist in the package — " +
                           "the file would be dropped at commit. Ensure identification_documents.person_id is the person's id (UUID).");

            await CheckFileAsync(doc.FilePath, doc.OriginalFileName, warnings);

            if (ApplyResults(doc, errors, warnings))
            {
                totalErrors += errors.Count;
                totalWarnings += warnings.Count;
            }
            totalChecked++;
        }

        await SaveModifiedAsync(_buildingDocRepo, buildingDocs, ct);
        await SaveModifiedAsync(_idDocRepo, idDocs, ct);

        return new ValidatorResult
        {
            ValidatorName = Name, Level = Level,
            ErrorCount = totalErrors, WarningCount = totalWarnings,
            RecordsChecked = totalChecked, Duration = sw.Elapsed
        };
    }

    /// <summary>
    /// Warn when the document's file was not extracted to server storage. A correctly built
    /// package embeds the file as a BLOB in the .uhc attachments table, which staging extracts
    /// under the uploads root; a tablet-local path (no BLOB) won't resolve here.
    /// </summary>
    private async Task CheckFileAsync(string? filePath, string? fileName, List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            warnings.Add($"No file stored for '{fileName}' — the .uhc package did not embed it as a BLOB in the attachments table. " +
                         "The record will commit as metadata only and the file will not be downloadable.");
            return;
        }

        bool exists;
        try
        {
            exists = await _fileStorageService.FileExistsAsync(filePath);
        }
        catch
        {
            // ResolveSafePath rejects paths outside the uploads root (e.g. a raw tablet path).
            exists = false;
        }

        if (!exists)
            warnings.Add($"File '{fileName}' is not present in server storage ({filePath}) — " +
                         "it will not be downloadable. Ensure the file is embedded as a BLOB in the attachments table.");
    }

    /// <summary>
    /// Merge new errors/warnings with any recorded by earlier validators and re-mark the entity.
    /// Returns true if the entity was modified (so the caller counts and persists it).
    /// </summary>
    private static bool ApplyResults(BaseStagingEntity entity, List<string> errors, List<string> warnings)
    {
        if (errors.Count == 0 && warnings.Count == 0)
            return false;

        var existingErrors = Deserialize(entity.ValidationErrors);
        var existingWarnings = Deserialize(entity.ValidationWarnings);
        existingErrors.AddRange(errors);
        existingWarnings.AddRange(warnings);

        if (existingErrors.Count > 0)
        {
            entity.MarkAsInvalid(
                JsonSerializer.Serialize(existingErrors),
                existingWarnings.Count > 0 ? JsonSerializer.Serialize(existingWarnings) : null);
        }
        else
        {
            entity.MarkAsValid(JsonSerializer.Serialize(existingWarnings));
        }

        return true;
    }

    private static List<string> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
        catch { return new List<string>(); }
    }

    private static async Task SaveModifiedAsync<T>(
        IStagingRepository<T> repo, List<T> entities, CancellationToken ct)
        where T : BaseStagingEntity
    {
        var modified = entities.Where(e =>
            e.ValidationStatus == Domain.Enums.StagingValidationStatus.Invalid ||
            e.ValidationStatus == Domain.Enums.StagingValidationStatus.Warning).ToList();
        if (modified.Count > 0)
        {
            await repo.UpdateRangeAsync(modified, ct);
            await repo.SaveChangesAsync(ct);
        }
    }
}
