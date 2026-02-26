using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Commands.UploadPackage;

/// <summary>
/// Handler for UploadPackageCommand.
/// Orchestrates the full package intake pipeline:
///   1. Save .uhc file to temp storage
///   2. Parse manifest from SQLite
///   3. Idempotency check (duplicate PackageId)
///   4. Compute and verify SHA-256 checksum
///   5. Verify digital signature (if required)
///   6. Check vocabulary compatibility
///   7. Create ImportPackage entity with status=Pending
///   8. On failure → Quarantine
/// 
/// UC-003 Stage 2 — S12 (Verify Package Integrity).
/// </summary>
public class UploadPackageCommandHandler : IRequestHandler<UploadPackageCommand, UploadPackageResultDto>
{
    private readonly IImportService _importService;
    private readonly IImportPackageRepository _packageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UploadPackageCommandHandler> _logger;

    public UploadPackageCommandHandler(
        IImportService importService,
        IImportPackageRepository packageRepository,
        ICurrentUserService currentUserService,
        ILogger<UploadPackageCommandHandler> logger)
    {
        _importService = importService;
        _packageRepository = packageRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UploadPackageResultDto> Handle(
        UploadPackageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        string? savedFilePath = null;

        try
        {
            // ============================================================
            // STEP 1: Save .uhc file to temporary storage
            // ============================================================
            _logger.LogInformation("Saving uploaded package: {FileName} ({Size} bytes)",
                request.FileName, request.FileSizeBytes);

            savedFilePath = await _importService.SavePackageFileAsync(
                request.FileStream, request.FileName, cancellationToken);

            // ============================================================
            // STEP 2: Parse manifest from SQLite
            // ============================================================
            _logger.LogInformation("Parsing manifest from: {FilePath}", savedFilePath);

            var manifest = await _importService.ParseManifestAsync(savedFilePath, cancellationToken);

            _logger.LogInformation(
                "Manifest parsed: PackageId={PackageId}, Records={Total}, Schema={Schema}",
                manifest.PackageId, manifest.TotalRecordCount, manifest.SchemaVersion);

            // ============================================================
            // STEP 3: Idempotency check — has this package been imported before?
            // ============================================================
            var existingPackage = await _packageRepository.GetByPackageIdAsync(
                manifest.PackageId, cancellationToken);

            if (existingPackage != null)
            {
                _logger.LogWarning(
                    "Duplicate package detected: PackageId={PackageId}, ExistingStatus={Status}",
                    manifest.PackageId, existingPackage.Status);

                // Best-effort cleanup of the temp file — never let a cleanup failure
                // mask the real (duplicate) result from the caller.
                try
                {
                    await _importService.DeletePackageFileAsync(savedFilePath, cancellationToken);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx,
                        "Could not delete temp file for duplicate package (will be cleaned up later): {Path}",
                        savedFilePath);
                }

                return new UploadPackageResultDto
                {
                    Package = MapToDto(existingPackage),
                    ValidationResult = new PackageValidationResultDto { IsValid = true },
                    IsDuplicatePackage = true,
                    IsQuarantined = false,
                    Message = $"Package already imported with status: {existingPackage.Status}. " +
                              $"Existing package ID: {existingPackage.Id}"
                };
            }

            // ============================================================
            // STEP 4: Compute and verify content checksum
            // ============================================================
            // The manifest's checksum covers all data tables EXCEPT the manifest
            // itself (avoiding the circular dependency where the checksum field's
            // presence in SQLite would change the file hash).
            _logger.LogInformation("Computing content checksum (data tables only, excluding manifest)...");

            var computedChecksum = await _importService.ComputeContentChecksumAsync(
                savedFilePath, cancellationToken);

            var isChecksumValid = string.IsNullOrEmpty(manifest.Checksum)
                || string.Equals(computedChecksum, manifest.Checksum, StringComparison.OrdinalIgnoreCase);

            if (!isChecksumValid)
            {
                _logger.LogWarning(
                    "Content checksum mismatch! Computed={Computed}, Manifest={Expected}. " +
                    "Package may have been tampered with or corrupted during transfer.",
                    computedChecksum, manifest.Checksum);
            }
            else
            {
                _logger.LogInformation(
                    "Content checksum verified: {Checksum}", computedChecksum);
            }

            // ============================================================
            // STEP 5: Verify digital signature
            // ============================================================
            bool isSignatureValid;
            using (var fileStreamForSignature = File.OpenRead(savedFilePath))
            {
                isSignatureValid = await _importService.VerifyDigitalSignatureAsync(
                    fileStreamForSignature, manifest.DigitalSignature, cancellationToken);
            }

            // ============================================================
            // STEP 6: Check vocabulary compatibility
            // ============================================================
            var vocabResult = await _importService.CheckVocabularyCompatibilityAsync(manifest, cancellationToken);

            _logger.LogInformation(
                "Vocabulary check: Compatible={IsCompatible}, FullyCompatible={IsFully}",
                vocabResult.IsCompatible, vocabResult.IsFullyCompatible);

            // ============================================================
            // STEP 7: Create ImportPackage entity
            // ============================================================
            var package = ImportPackage.Create(
                packageId: manifest.PackageId,
                fileName: request.FileName,
                fileSizeBytes: request.FileSizeBytes,
                checksum: computedChecksum,
                packageCreatedDate: manifest.CreatedUtc,
                packageExportedDate: manifest.ExportedDateUtc,
                exportedByUserId: manifest.ExportedByUserId,
                deviceId: manifest.DeviceId,
                surveyCount: manifest.SurveyCount,
                buildingCount: manifest.BuildingCount,
                propertyUnitCount: manifest.PropertyUnitCount,
                personCount: manifest.PersonCount,
                claimCount: manifest.ClaimCount,
                documentCount: manifest.DocumentCount,
                totalAttachmentSizeBytes: manifest.TotalAttachmentSizeBytes,
                createdByUserId: userId);

            // Record who imported and how
            package.MarkAsImported(userId, request.ImportMethod, userId);

            // Set security validation results
            package.SetSecurityValidation(
                isChecksumValid, isSignatureValid, manifest.DigitalSignature, userId);

            // Set schema validation (basic check: schema version is not empty)
            var isSchemaValid = !string.IsNullOrWhiteSpace(manifest.SchemaVersion);
            package.SetSchemaValidation(isSchemaValid, manifest.SchemaVersion, userId);

            // Set vocabulary compatibility
            package.SetVocabularyCompatibility(
                vocabResult.IsCompatible,
                vocabResult.VersionsJson,
                vocabResult.IssuesJson,
                userId);

            // ============================================================
            // STEP 8: Determine outcome — Quarantine or accept
            // ============================================================
            var validationResult = new PackageValidationResultDto
            {
                IsChecksumValid = isChecksumValid,
                IsSignatureValid = isSignatureValid,
                IsSchemaValid = isSchemaValid,
                IsVocabularyCompatible = vocabResult.IsCompatible,
                VocabularyWarnings = vocabResult.Items
                    .Where(i => i.Level == Models.VocabularyCompatibilityLevel.MinorDifference)
                    .Select(i => i.Message ?? $"Minor version difference in {i.Domain}")
                    .ToList()
            };

            var isQuarantined = false;

            if (!isChecksumValid)
            {
                package.Quarantine("SHA-256 checksum verification failed", userId);
                validationResult.Errors.Add("Checksum verification failed — file may be corrupted or tampered with");
                isQuarantined = true;
            }
            else if (!isSignatureValid)
            {
                package.Quarantine("Digital signature verification failed", userId);
                validationResult.Errors.Add("Digital signature verification failed");
                isQuarantined = true;
            }
            else if (!vocabResult.IsCompatible)
            {
                package.Quarantine("Incompatible vocabulary versions (MAJOR version mismatch)", userId);
                validationResult.Errors.Add(
                    $"Vocabulary version incompatibility: {vocabResult.Summary}");
                isQuarantined = true;
            }

            validationResult.IsValid = !isQuarantined;

            // Non-blocking warnings
            if (!vocabResult.IsFullyCompatible && vocabResult.IsCompatible)
            {
                validationResult.Warnings.Add(
                    $"Vocabulary minor differences detected: {vocabResult.Summary}");
            }

            // Persist
            await _packageRepository.AddAsync(package, cancellationToken);
            await _packageRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Package created: Id={Id}, PackageId={PackageId}, Status={Status}, Quarantined={IsQuarantined}",
                package.Id, package.PackageId, package.Status, isQuarantined);

            return new UploadPackageResultDto
            {
                Package = MapToDto(package),
                ValidationResult = validationResult,
                IsQuarantined = isQuarantined,
                IsDuplicatePackage = false,
                Message = isQuarantined
                    ? $"Package quarantined: {validationResult.Errors.FirstOrDefault()}"
                    : $"Package accepted. {manifest.TotalRecordCount} records ready for staging."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process package upload: {FileName}", request.FileName);

            // Clean up temp file on unexpected failure
            if (savedFilePath != null)
            {
                try
                {
                    await _importService.DeletePackageFileAsync(savedFilePath, cancellationToken);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Failed to clean up temp file: {Path}", savedFilePath);
                }
            }

            throw;
        }
    }

    // ==================== MAPPING ====================

    private static ImportPackageDto MapToDto(ImportPackage entity)
    {
        return new ImportPackageDto
        {
            Id = entity.Id,
            PackageId = entity.PackageId,
            PackageNumber = entity.PackageNumber,
            FileName = entity.FileName,
            FileSizeBytes = entity.FileSizeBytes,
            PackageCreatedDate = entity.PackageCreatedDate,
            PackageExportedDate = entity.PackageExportedDate,
            ExportedByUserId = entity.ExportedByUserId,
            DeviceId = entity.DeviceId,
            Status = (int)entity.Status,
            ImportedDate = entity.ImportedDate,
            ImportedByUserId = entity.ImportedByUserId,
            ValidationStartedDate = entity.ValidationStartedDate,
            ValidationCompletedDate = entity.ValidationCompletedDate,
            CommittedDate = entity.CommittedDate,
            IsChecksumValid = entity.IsChecksumValid,
            IsSignatureValid = entity.IsSignatureValid,
            SurveyCount = entity.SurveyCount,
            BuildingCount = entity.BuildingCount,
            PropertyUnitCount = entity.PropertyUnitCount,
            PersonCount = entity.PersonCount,
            ClaimCount = entity.ClaimCount,
            DocumentCount = entity.DocumentCount,
            TotalAttachmentSizeBytes = entity.TotalAttachmentSizeBytes,
            IsVocabularyCompatible = entity.IsVocabularyCompatible,
            VocabularyCompatibilityIssues = entity.VocabularyCompatibilityIssues,
            IsSchemaValid = entity.IsSchemaValid,
            ValidationErrorCount = entity.ValidationErrorCount,
            ValidationWarningCount = entity.ValidationWarningCount,
            PersonDuplicateCount = entity.PersonDuplicateCount,
            PropertyDuplicateCount = entity.PropertyDuplicateCount,
            ConflictCount = entity.ConflictCount,
            AreConflictsResolved = entity.AreConflictsResolved,
            SuccessfulImportCount = entity.SuccessfulImportCount,
            FailedImportCount = entity.FailedImportCount,
            SkippedRecordCount = entity.SkippedRecordCount,
            SuccessRate = entity.GetSuccessRate(),
            ErrorMessage = entity.ErrorMessage,
            IsArchived = entity.IsArchived,
            CreatedAtUtc = entity.CreatedAtUtc,
            LastModifiedAtUtc = entity.LastModifiedAtUtc
        };
    }
}
