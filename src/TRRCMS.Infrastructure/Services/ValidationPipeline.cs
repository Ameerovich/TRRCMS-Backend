using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Orchestrates the 8-level validation pipeline for staged import data.
/// Validators are resolved from DI and run in order (Level 1→8).
/// Each validator writes errors/warnings directly to staging entities.
/// After all validators run, this class aggregates the final status
/// counts across all entity types.
///
/// FSD: FR-D-4 (Staging & Validation).
/// </summary>
public class ValidationPipeline : IValidationPipeline
{
    private readonly IEnumerable<IStagingValidator> _validators;
    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _unitRepo;
    private readonly IStagingRepository<StagingPerson> _personRepo;
    private readonly IStagingRepository<StagingHousehold> _householdRepo;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _relationRepo;
    private readonly IStagingRepository<StagingEvidence> _evidenceRepo;
    private readonly IStagingRepository<StagingClaim> _claimRepo;
    private readonly IStagingRepository<StagingSurvey> _surveyRepo;
    private readonly ILogger<ValidationPipeline> _logger;

    public ValidationPipeline(
        IEnumerable<IStagingValidator> validators,
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPropertyUnit> unitRepo,
        IStagingRepository<StagingPerson> personRepo,
        IStagingRepository<StagingHousehold> householdRepo,
        IStagingRepository<StagingPersonPropertyRelation> relationRepo,
        IStagingRepository<StagingEvidence> evidenceRepo,
        IStagingRepository<StagingClaim> claimRepo,
        IStagingRepository<StagingSurvey> surveyRepo,
        ILogger<ValidationPipeline> logger)
    {
        _validators = validators;
        _buildingRepo = buildingRepo;
        _unitRepo = unitRepo;
        _personRepo = personRepo;
        _householdRepo = householdRepo;
        _relationRepo = relationRepo;
        _evidenceRepo = evidenceRepo;
        _claimRepo = claimRepo;
        _surveyRepo = surveyRepo;
        _logger = logger;
    }

    public async Task<ValidationSummary> ValidateAsync(
        Guid importPackageId, CancellationToken cancellationToken = default)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var levelResults = new List<ValidatorResult>();

        // Sort validators by level and run them in order
        var orderedValidators = _validators.OrderBy(v => v.Level).ToList();

        _logger.LogInformation(
            "Running {Count} validators for package {PackageId}",
            orderedValidators.Count, importPackageId);

        foreach (var validator in orderedValidators)
        {
            _logger.LogDebug("Running Level {Level}: {Name}", validator.Level, validator.Name);

            try
            {
                var result = await validator.ValidateAsync(importPackageId, cancellationToken);
                levelResults.Add(result);

                _logger.LogDebug(
                    "Level {Level} ({Name}): {Errors} errors, {Warnings} warnings, " +
                    "{Checked} records in {Duration}ms",
                    result.Level, result.ValidatorName,
                    result.ErrorCount, result.WarningCount,
                    result.RecordsChecked, result.Duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validator Level {Level} ({Name}) failed", validator.Level, validator.Name);

                levelResults.Add(new ValidatorResult
                {
                    ValidatorName = validator.Name,
                    Level = validator.Level,
                    ErrorCount = -1, // indicates failure
                    WarningCount = 0,
                    RecordsChecked = 0,
                    Duration = TimeSpan.Zero
                });
            }
        }

        // After all validators have run, finalize entity statuses.
        // Entities that still have Pending status and no errors → mark as Valid.
        await FinalizeEntityStatusesAsync(importPackageId, cancellationToken);

        // Aggregate counts across all entity types
        var summary = await BuildSummaryAsync(importPackageId, levelResults, cancellationToken);
        summary.TotalDuration = totalStopwatch.Elapsed;

        return summary;
    }

    /// <summary>
    /// After all validators run, entities still in Pending status with no errors
    /// should be marked as Valid (they passed all checks).
    /// </summary>
    private async Task FinalizeEntityStatusesAsync(
        Guid importPackageId, CancellationToken ct)
    {
        await FinalizeForTypeAsync(_buildingRepo, importPackageId, ct);
        await FinalizeForTypeAsync(_unitRepo, importPackageId, ct);
        await FinalizeForTypeAsync(_personRepo, importPackageId, ct);
        await FinalizeForTypeAsync(_householdRepo, importPackageId, ct);
        await FinalizeForTypeAsync(_relationRepo, importPackageId, ct);
        await FinalizeForTypeAsync(_evidenceRepo, importPackageId, ct);
        await FinalizeForTypeAsync(_claimRepo, importPackageId, ct);
        await FinalizeForTypeAsync(_surveyRepo, importPackageId, ct);
    }

    private static async Task FinalizeForTypeAsync<T>(
        IStagingRepository<T> repo, Guid importPackageId, CancellationToken ct)
        where T : BaseStagingEntity
    {
        var pendingEntities = await repo.GetByPackageAndStatusAsync(
            importPackageId, StagingValidationStatus.Pending, ct);

        foreach (var entity in pendingEntities)
        {
            entity.MarkAsValid();
        }

        if (pendingEntities.Count > 0)
        {
            await repo.UpdateRangeAsync(pendingEntities, ct);
            await repo.SaveChangesAsync(ct);
        }
    }

    private async Task<ValidationSummary> BuildSummaryAsync(
        Guid importPackageId, List<ValidatorResult> levelResults, CancellationToken ct)
    {
        var summary = new ValidationSummary
        {
            ImportPackageId = importPackageId,
            LevelResults = levelResults
        };

        // Sum counts across all 8 entity types
        await AddCountsAsync(_buildingRepo, importPackageId, summary, ct);
        await AddCountsAsync(_unitRepo, importPackageId, summary, ct);
        await AddCountsAsync(_personRepo, importPackageId, summary, ct);
        await AddCountsAsync(_householdRepo, importPackageId, summary, ct);
        await AddCountsAsync(_relationRepo, importPackageId, summary, ct);
        await AddCountsAsync(_evidenceRepo, importPackageId, summary, ct);
        await AddCountsAsync(_claimRepo, importPackageId, summary, ct);
        await AddCountsAsync(_surveyRepo, importPackageId, summary, ct);

        return summary;
    }

    private static async Task AddCountsAsync<T>(
        IStagingRepository<T> repo, Guid importPackageId,
        ValidationSummary summary, CancellationToken ct)
        where T : BaseStagingEntity
    {
        var counts = await repo.GetStatusCountsByPackageAsync(importPackageId, ct);
        summary.TotalRecords += counts.Values.Sum();
        summary.ValidCount += counts.GetValueOrDefault(StagingValidationStatus.Valid, 0);
        summary.InvalidCount += counts.GetValueOrDefault(StagingValidationStatus.Invalid, 0);
        summary.WarningCount += counts.GetValueOrDefault(StagingValidationStatus.Warning, 0);
        summary.SkippedCount += counts.GetValueOrDefault(StagingValidationStatus.Skipped, 0);
        summary.PendingCount += counts.GetValueOrDefault(StagingValidationStatus.Pending, 0);
    }
}
