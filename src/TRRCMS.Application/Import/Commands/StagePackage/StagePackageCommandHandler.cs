using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Commands.StagePackage;

/// <summary>
/// Handler for StagePackageCommand.
/// Orchestrates:
///   1. Validate package exists and is in correct status
///   2. If retrying, cleanup previous staging data
///   3. Unpack .uhc → staging tables (IStagingService)
///   4. Run 8-level validation pipeline (IValidationPipeline)
///   5. Update ImportPackage with validation results
///   6. Return StagingSummaryDto
///
/// UC-003 Stage 2 — S13/S14.
/// </summary>
public class StagePackageCommandHandler : IRequestHandler<StagePackageCommand, StagingSummaryDto>
{
    private readonly IImportPackageRepository _packageRepository;
    private readonly IStagingService _stagingService;
    private readonly IValidationPipeline _validationPipeline;
    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _unitRepo;
    private readonly IStagingRepository<StagingPerson> _personRepo;
    private readonly IStagingRepository<StagingHousehold> _householdRepo;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _relationRepo;
    private readonly IStagingRepository<StagingEvidence> _evidenceRepo;
    private readonly IStagingRepository<StagingClaim> _claimRepo;
    private readonly IStagingRepository<StagingSurvey> _surveyRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<StagePackageCommandHandler> _logger;

    public StagePackageCommandHandler(
        IImportPackageRepository packageRepository,
        IStagingService stagingService,
        IValidationPipeline validationPipeline,
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPropertyUnit> unitRepo,
        IStagingRepository<StagingPerson> personRepo,
        IStagingRepository<StagingHousehold> householdRepo,
        IStagingRepository<StagingPersonPropertyRelation> relationRepo,
        IStagingRepository<StagingEvidence> evidenceRepo,
        IStagingRepository<StagingClaim> claimRepo,
        IStagingRepository<StagingSurvey> surveyRepo,
        ICurrentUserService currentUserService,
        ILogger<StagePackageCommandHandler> logger)
    {
        _packageRepository = packageRepository;
        _stagingService = stagingService;
        _validationPipeline = validationPipeline;
        _buildingRepo = buildingRepo;
        _unitRepo = unitRepo;
        _personRepo = personRepo;
        _householdRepo = householdRepo;
        _relationRepo = relationRepo;
        _evidenceRepo = evidenceRepo;
        _claimRepo = claimRepo;
        _surveyRepo = surveyRepo;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<StagingSummaryDto> Handle(
        StagePackageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        // ============================================================
        // STEP 1: Load and validate package
        // ============================================================
        var package = await _packageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException(
                $"Import package not found: {request.ImportPackageId}");

        // Allow staging from Validating (fresh upload) or ValidationFailed (retry)
        if (package.Status != ImportStatus.Validating &&
            package.Status != ImportStatus.ValidationFailed)
        {
            throw new ConflictException(
                $"Cannot stage package with status '{package.Status}'. " +
                $"Expected Validating or ValidationFailed.");
        }

        _logger.LogInformation(
            "Starting staging for package {PackageId} (Id={Id}, Status={Status})",
            package.PackageId, package.Id, package.Status);

        // ============================================================
        // STEP 2: If retrying, cleanup previous staging data
        // ============================================================
        if (package.Status == ImportStatus.ValidationFailed)
        {
            _logger.LogInformation("Retry detected — cleaning up previous staging data");
            await _stagingService.CleanupStagingAsync(package.Id, cancellationToken);
        }

        try
        {
            // ============================================================
            // STEP 3: Update status → Staging
            // ============================================================
            package.AddValidationResults(null, null, 0, 0, userId);

            // Note: AddValidationResults transitions to Staging when errorCount=0.
            // The status is now ImportStatus.Staging.

            await _packageRepository.UpdateAsync(package, cancellationToken);
            await _packageRepository.SaveChangesAsync(cancellationToken);

            // ============================================================
            // STEP 4: Unpack .uhc → staging tables
            // ============================================================
            // The .uhc file path — stored during upload in the package storage path.
            // We retrieve it by convention: {PackageStoragePath}/{PackageId}.uhc
            // or from the ArchivePath if already archived.
            var uhcFilePath = ResolveUhcFilePath(package);

            _logger.LogInformation("Unpacking .uhc from: {FilePath}", uhcFilePath);

            var stagingResult = await _stagingService.UnpackAndStageAsync(
                package.Id, uhcFilePath, cancellationToken);

            _logger.LogInformation(
                "Staging complete: {Total} records ({Surveys}S, {Buildings}B, {Units}U, " +
                "{Persons}P, {Households}H, {Relations}R, {Evidence}E, {Claims}C)",
                stagingResult.TotalRecordCount,
                stagingResult.SurveyCount, stagingResult.BuildingCount,
                stagingResult.PropertyUnitCount, stagingResult.PersonCount,
                stagingResult.HouseholdCount, stagingResult.PersonPropertyRelationCount,
                stagingResult.EvidenceCount, stagingResult.ClaimCount);

            // ============================================================
            // STEP 5: Run 8-level validation pipeline
            // ============================================================
            _logger.LogInformation("Starting validation pipeline...");

            var validationSummary = await _validationPipeline.ValidateAsync(
                package.Id, cancellationToken);

            _logger.LogInformation(
                "Validation complete: {Valid} valid, {Invalid} invalid, " +
                "{Warning} warnings, {Skipped} skipped (in {Duration}ms)",
                validationSummary.ValidCount, validationSummary.InvalidCount,
                validationSummary.WarningCount, validationSummary.SkippedCount,
                validationSummary.TotalDuration.TotalMilliseconds);

            // ============================================================
            // STEP 6: Update ImportPackage with validation results
            // ============================================================
            var errorsJson = validationSummary.InvalidCount > 0
                ? JsonSerializer.Serialize(validationSummary.LevelResults
                    .Where(r => r.ErrorCount > 0)
                    .Select(r => new { r.ValidatorName, r.Level, r.ErrorCount })
                    .ToList())
                : null;

            var warningsJson = validationSummary.WarningCount > 0
                ? JsonSerializer.Serialize(validationSummary.LevelResults
                    .Where(r => r.WarningCount > 0)
                    .Select(r => new { r.ValidatorName, r.Level, r.WarningCount })
                    .ToList())
                : null;

            package.AddValidationResults(
                errorsJson,
                warningsJson,
                validationSummary.InvalidCount,
                validationSummary.WarningCount,
                userId);

            // AddValidationResults transitions to ValidationFailed if errors > 0,
            // or to Staging if errors = 0. We want Staging to proceed to duplicate detection.

            await _packageRepository.UpdateAsync(package, cancellationToken);
            await _packageRepository.SaveChangesAsync(cancellationToken);

            // ============================================================
            // STEP 7: Build response DTO
            // ============================================================
            var summary = await BuildSummaryDtoAsync(
                package.Id, package.PackageNumber, package.Status.ToString(),
                stagingResult, validationSummary, cancellationToken);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Staging failed for package {PackageId}", package.PackageId);

            // Mark package as failed
            package.MarkAsFailed(
                $"Staging failed: {ex.Message}",
                JsonSerializer.Serialize(new { ex.Message, ex.StackTrace }),
                userId);

            await _packageRepository.UpdateAsync(package, cancellationToken);
            await _packageRepository.SaveChangesAsync(cancellationToken);

            throw;
        }
    }

    // ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// Resolve the file system path to the .uhc file for this package.
    /// Looks for the file in the package storage directory.
    /// </summary>
    private static string ResolveUhcFilePath(Domain.Entities.ImportPackage package)
    {
        // The UploadPackageCommandHandler saves files as {guid}_{filename}.uhc
        // and the path is stored indirectly via the ArchivePath or can be found
        // by convention. For now, if ArchivePath is set, use that. Otherwise,
        // the file is in the package storage path (configured in ImportPipelineSettings).
        if (!string.IsNullOrWhiteSpace(package.ArchivePath) && File.Exists(package.ArchivePath))
        {
            return package.ArchivePath;
        }

        // Fallback: look in wwwroot/packages for any file matching the PackageId
        var storagePath = Path.Combine("wwwroot", "packages");
        if (Directory.Exists(storagePath))
        {
            var matchingFile = Directory.GetFiles(storagePath, $"*{package.PackageId:N}*")
                .FirstOrDefault();
            if (matchingFile != null)
                return matchingFile;

            // Also try matching by filename
            var byName = Directory.GetFiles(storagePath, $"*{package.FileName}*")
                .FirstOrDefault();
            if (byName != null)
                return byName;
        }

        throw new FileNotFoundException(
            $"Cannot find .uhc file for package {package.PackageId}. " +
            $"Expected in '{storagePath}' or ArchivePath='{package.ArchivePath}'");
    }

    /// <summary>
    /// Build the StagingSummaryDto with per-entity-type counts.
    /// </summary>
    private async Task<StagingSummaryDto> BuildSummaryDtoAsync(
        Guid importPackageId,
        string packageNumber,
        string status,
        StagingResult stagingResult,
        ValidationSummary validationSummary,
        CancellationToken cancellationToken)
    {
        var dto = new StagingSummaryDto
        {
            ImportPackageId = importPackageId,
            PackageNumber = packageNumber,
            Status = status,
            TotalRecords = validationSummary.TotalRecords,
            TotalValid = validationSummary.ValidCount,
            TotalInvalid = validationSummary.InvalidCount,
            TotalWarning = validationSummary.WarningCount,
            TotalSkipped = validationSummary.SkippedCount,
            TotalPending = validationSummary.PendingCount,
            AttachmentFilesExtracted = stagingResult.AttachmentFilesExtracted,
            AttachmentBytesExtracted = stagingResult.AttachmentBytesExtracted,
            LevelResults = validationSummary.LevelResults.Select(r => new ValidationLevelResultDto
            {
                Level = r.Level,
                ValidatorName = r.ValidatorName,
                ErrorCount = r.ErrorCount,
                WarningCount = r.WarningCount,
                RecordsChecked = r.RecordsChecked,
                DurationMs = r.Duration.TotalMilliseconds
            }).ToList()
        };

        // Per-entity-type counts
        dto.Buildings = await BuildEntitySummaryAsync(_buildingRepo, "Building", importPackageId, cancellationToken);
        dto.PropertyUnits = await BuildEntitySummaryAsync(_unitRepo, "PropertyUnit", importPackageId, cancellationToken);
        dto.Persons = await BuildEntitySummaryAsync(_personRepo, "Person", importPackageId, cancellationToken);
        dto.Households = await BuildEntitySummaryAsync(_householdRepo, "Household", importPackageId, cancellationToken);
        dto.PersonPropertyRelations = await BuildEntitySummaryAsync(_relationRepo, "PersonPropertyRelation", importPackageId, cancellationToken);
        dto.Evidences = await BuildEntitySummaryAsync(_evidenceRepo, "Evidence", importPackageId, cancellationToken);
        dto.Claims = await BuildEntitySummaryAsync(_claimRepo, "Claim", importPackageId, cancellationToken);
        dto.Surveys = await BuildEntitySummaryAsync(_surveyRepo, "Survey", importPackageId, cancellationToken);

        return dto;
    }

    private static async Task<EntityTypeSummary> BuildEntitySummaryAsync<T>(
        IStagingRepository<T> repo,
        string entityTypeName,
        Guid importPackageId,
        CancellationToken cancellationToken) where T : Domain.Common.BaseStagingEntity
    {
        var counts = await repo.GetStatusCountsByPackageAsync(importPackageId, cancellationToken);

        return new EntityTypeSummary
        {
            EntityType = entityTypeName,
            Total = counts.Values.Sum(),
            Valid = counts.GetValueOrDefault(StagingValidationStatus.Valid, 0),
            Invalid = counts.GetValueOrDefault(StagingValidationStatus.Invalid, 0),
            Warning = counts.GetValueOrDefault(StagingValidationStatus.Warning, 0),
            Skipped = counts.GetValueOrDefault(StagingValidationStatus.Skipped, 0),
            Pending = counts.GetValueOrDefault(StagingValidationStatus.Pending, 0)
        };
    }

}
