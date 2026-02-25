using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;
using TRRCMS.Domain.ValueObjects;
using TRRCMS.Infrastructure.Services.Matching;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Orchestrates duplicate detection for a staged import package.
/// Delegates to <see cref="PersonMatchingService"/> and <see cref="PropertyMatchingService"/>
/// for the actual comparison logic, then creates <see cref="ConflictResolution"/> entities
/// for each detected duplicate pair.
///
/// FSD: FR-D-5 (Person), FR-D-6 (Property), FR-D-7 (Conflict Resolution).
/// UC-003 Stage 2 — S14.
/// </summary>
public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly IStagingRepository<StagingPerson> _stagingPersonRepo;
    private readonly IStagingRepository<StagingBuilding> _stagingBuildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _stagingUnitRepo;
    private readonly IConflictResolutionRepository _conflictRepository;
    private readonly PersonMatchingService _personMatcher;
    private readonly PropertyMatchingService _propertyMatcher;
    private readonly ILogger<DuplicateDetectionService> _logger;

    public DuplicateDetectionService(
        IStagingRepository<StagingPerson> stagingPersonRepo,
        IStagingRepository<StagingBuilding> stagingBuildingRepo,
        IStagingRepository<StagingPropertyUnit> stagingUnitRepo,
        IConflictResolutionRepository conflictRepository,
        PersonMatchingService personMatcher,
        PropertyMatchingService propertyMatcher,
        ILogger<DuplicateDetectionService> logger)
    {
        _stagingPersonRepo = stagingPersonRepo;
        _stagingBuildingRepo = stagingBuildingRepo;
        _stagingUnitRepo = stagingUnitRepo;
        _conflictRepository = conflictRepository;
        _personMatcher = personMatcher;
        _propertyMatcher = propertyMatcher;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DuplicateDetectionResult> DetectAsync(
        Guid importPackageId,
        Guid detectedByUserId,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new DuplicateDetectionResult();

        _logger.LogInformation(
            "Starting duplicate detection for package {PackageId}", importPackageId);

        // ============================================================
        // Load staging data (only Valid and Warning records are candidates)
        // ============================================================
        var stagingPersons = await LoadCommittablePersonsAsync(importPackageId, cancellationToken);
        var stagingBuildings = await LoadCommittableBuildingsAsync(importPackageId, cancellationToken);
        var stagingUnits = await LoadCommittableUnitsAsync(importPackageId, cancellationToken);

        result.PersonsScanned = stagingPersons.Count;
        result.BuildingsScanned = stagingBuildings.Count;

        // ============================================================
        // Person duplicate detection
        // ============================================================
        var personMatches = await _personMatcher.DetectDuplicatesAsync(
            stagingPersons, cancellationToken);

        var personConflicts = await CreatePersonConflictsAsync(
            personMatches, importPackageId, detectedByUserId, cancellationToken);

        result.PersonDuplicatesFound = personConflicts.Count;

        // ============================================================
        // Property duplicate detection
        // ============================================================
        var propertyMatches = await _propertyMatcher.DetectDuplicatesAsync(
            stagingBuildings, stagingUnits, cancellationToken);

        var propertyConflicts = await CreatePropertyConflictsAsync(
            propertyMatches, importPackageId, detectedByUserId, cancellationToken);

        result.PropertyDuplicatesFound = propertyConflicts.Count;

        // ============================================================
        // Aggregate results
        // ============================================================
        var allConflicts = personConflicts.Concat(propertyConflicts).ToList();
        result.TotalConflictsCreated = allConflicts.Count;
        result.ConflictIds = allConflicts.Select(c => c.Id).ToList();
        result.Duration = sw.Elapsed;

        _logger.LogInformation(
            "Duplicate detection complete for package {PackageId}: " +
            "{PersonDupes} person, {PropertyDupes} property, {Total} total (in {Duration}ms)",
            importPackageId, result.PersonDuplicatesFound,
            result.PropertyDuplicatesFound, result.TotalConflictsCreated,
            result.Duration.TotalMilliseconds);

        return result;
    }

    // ==================== STAGING DATA LOADING ====================

    /// <summary>
    /// Load staging persons that are candidates for duplicate checking.
    /// Only Valid and Warning records are eligible (Invalid/Skipped won't be committed).
    /// </summary>
    private async Task<List<StagingPerson>> LoadCommittablePersonsAsync(
        Guid importPackageId, CancellationToken ct)
    {
        var valid = await _stagingPersonRepo
            .GetByPackageAndStatusAsync(importPackageId, StagingValidationStatus.Valid, ct);
        var warning = await _stagingPersonRepo
            .GetByPackageAndStatusAsync(importPackageId, StagingValidationStatus.Warning, ct);

        return valid.Concat(warning).ToList();
    }

    private async Task<List<StagingBuilding>> LoadCommittableBuildingsAsync(
        Guid importPackageId, CancellationToken ct)
    {
        var valid = await _stagingBuildingRepo
            .GetByPackageAndStatusAsync(importPackageId, StagingValidationStatus.Valid, ct);
        var warning = await _stagingBuildingRepo
            .GetByPackageAndStatusAsync(importPackageId, StagingValidationStatus.Warning, ct);

        return valid.Concat(warning).ToList();
    }

    private async Task<List<StagingPropertyUnit>> LoadCommittableUnitsAsync(
        Guid importPackageId, CancellationToken ct)
    {
        var valid = await _stagingUnitRepo
            .GetByPackageAndStatusAsync(importPackageId, StagingValidationStatus.Valid, ct);
        var warning = await _stagingUnitRepo
            .GetByPackageAndStatusAsync(importPackageId, StagingValidationStatus.Warning, ct);

        return valid.Concat(warning).ToList();
    }

    // ==================== CONFLICT CREATION ====================

    /// <summary>
    /// Create ConflictResolution entities for person duplicate matches.
    /// Checks for existing conflict pairs to avoid duplicate conflict records.
    /// </summary>
    private async Task<List<ConflictResolution>> CreatePersonConflictsAsync(
        List<PersonMatchResult> matches,
        Guid importPackageId,
        Guid detectedByUserId,
        CancellationToken ct)
    {
        var conflicts = new List<ConflictResolution>();

        foreach (var match in matches)
        {
            // Check if a conflict already exists for this entity pair (order-independent)
            var existing = await _conflictRepository.GetByEntityPairAsync(
                match.StagingOriginalEntityId, match.MatchedEntityId, ct);

            if (existing != null)
            {
                _logger.LogDebug(
                    "Skipping duplicate conflict: {StagingId} ↔ {MatchedId} (already exists as {ConflictId})",
                    match.StagingOriginalEntityId, match.MatchedEntityId, existing.Id);
                continue;
            }

            var conflictType = match.IsWithinBatchMatch
                ? "PersonDuplicate_WithinBatch"
                : "PersonDuplicate";

            var description = match.NationalIdMatched
                ? $"National ID exact match detected (NID: {match.StagingPersonIdentifier})"
                : $"Composite similarity score {match.SimilarityScore}% " +
                  $"({match.ConfidenceLevel} confidence)";

            var conflict = ConflictResolution.Create(
                conflictType: conflictType,
                entityType: "Person",
                firstEntityId: match.StagingOriginalEntityId,
                secondEntityId: match.MatchedEntityId,
                firstEntityIdentifier: match.StagingPersonIdentifier,
                secondEntityIdentifier: match.MatchedPersonIdentifier,
                similarityScore: match.SimilarityScore,
                confidenceLevel: match.ConfidenceLevel,
                conflictDescription: description,
                matchingCriteriaJson: JsonSerializer.Serialize(match.ToMatchingCriteria()),
                dataComparisonJson: null, // Populated on-demand by GetConflictDetailsQuery (Step 6)
                isAutoDetected: true,
                importPackageId: importPackageId,
                createdByUserId: detectedByUserId);

            conflicts.Add(conflict);
        }

        if (conflicts.Count > 0)
        {
            await _conflictRepository.AddRangeAsync(conflicts, ct);
            await _conflictRepository.SaveChangesAsync(ct);
        }

        return conflicts;
    }

    /// <summary>
    /// Create ConflictResolution entities for property unit duplicate matches.
    /// Entity type is "PropertyUnit" — composite key: BuildingCode + UnitIdentifier.
    /// No building-level conflicts are created.
    /// </summary>
    private async Task<List<ConflictResolution>> CreatePropertyConflictsAsync(
        List<PropertyMatchResult> matches,
        Guid importPackageId,
        Guid detectedByUserId,
        CancellationToken ct)
    {
        var conflicts = new List<ConflictResolution>();

        foreach (var match in matches)
        {
            var existing = await _conflictRepository.GetByEntityPairAsync(
                match.StagingOriginalEntityId, match.MatchedEntityId, ct);

            if (existing != null)
                continue;

            var conflictType = match.IsWithinBatchMatch
                ? "PropertyDuplicate_WithinBatch"
                : "PropertyDuplicate";

            var description =
                $"PropertyUnit composite key exact match " +
                $"(BuildingCode: {match.BuildingCode}, UnitIdentifier: {match.UnitIdentifier})";

            var conflict = ConflictResolution.Create(
                conflictType: conflictType,
                entityType: "PropertyUnit",
                firstEntityId: match.StagingOriginalEntityId,
                secondEntityId: match.MatchedEntityId,
                firstEntityIdentifier: match.StagingUnitIdentifier,
                secondEntityIdentifier: match.MatchedUnitIdentifier,
                similarityScore: match.SimilarityScore,
                confidenceLevel: match.ConfidenceLevel,
                conflictDescription: description,
                matchingCriteriaJson: JsonSerializer.Serialize(match.ToMatchingCriteria()),
                dataComparisonJson: null, // Populated on-demand by GetConflictDetailsQuery
                isAutoDetected: true,
                importPackageId: importPackageId,
                createdByUserId: detectedByUserId);

            conflicts.Add(conflict);
        }

        if (conflicts.Count > 0)
        {
            await _conflictRepository.AddRangeAsync(conflicts, ct);
            await _conflictRepository.SaveChangesAsync(ct);
        }

        return conflicts;
    }
}
