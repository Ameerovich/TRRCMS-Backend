using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Infrastructure.Services.Matching;

/// <summary>
/// Detects duplicate PropertyUnit records by composite key:
///   BuildingCode (17-digit) + UnitIdentifier.
///
/// Detection runs at the PropertyUnit level only — no building-level duplicates
/// are raised because the same building legitimately appears across multiple
/// surveys and claims.
///
/// Two detection phases:
///   Phase 1 — Within-batch: two staging units in the same import share the
///             same composite key (BuildingCode + UnitIdentifier).
///   Phase 2 — Cross-batch:  a staging unit matches a production PropertyUnit
///             by the same composite key.
///
/// UC-007 (Resolve Duplicate Properties).
/// FSD: FR-D-6 (Property Matching), FR-D-7 (Conflict Resolution).
/// </summary>
public class PropertyMatchingService
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ILogger<PropertyMatchingService> _logger;

    public PropertyMatchingService(
        IBuildingRepository buildingRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ILogger<PropertyMatchingService> logger)
    {
        _buildingRepository = buildingRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _logger = logger;
    }

    /// <summary>
    /// Run property-unit duplicate detection for all valid/warning staging units.
    /// </summary>
    /// <param name="stagingBuildings">Staging buildings (used to resolve BuildingCode for each unit).</param>
    /// <param name="stagingUnits">Staging property units to check (already filtered to Valid/Warning).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of PropertyUnit match results (composite key matches only).</returns>
    public async Task<List<PropertyMatchResult>> DetectDuplicatesAsync(
        List<StagingBuilding> stagingBuildings,
        List<StagingPropertyUnit> stagingUnits,
        CancellationToken cancellationToken = default)
    {
        var results = new List<PropertyMatchResult>();

        if (stagingUnits.Count == 0)
            return results;

        // Build lookup: OriginalEntityId → BuildingCode (17-digit)
        // StagingPropertyUnit.OriginalBuildingId references StagingBuilding.OriginalEntityId
        var buildingCodeByOriginalId = stagingBuildings
            .Where(b => !string.IsNullOrWhiteSpace(b.BuildingId))
            .ToDictionary(b => b.OriginalEntityId, b => b.BuildingId!);

        // ============================================================
        // Phase 1: Within-batch duplicates
        // Two staging units in the same import sharing the same composite key.
        // ============================================================
        var withinBatchResults = DetectWithinBatchDuplicates(
            stagingUnits, buildingCodeByOriginalId);
        results.AddRange(withinBatchResults);

        _logger.LogDebug(
            "Within-batch property unit duplicates: {Count}", withinBatchResults.Count);

        // ============================================================
        // Phase 2: Cross-batch duplicates (staging vs production)
        // A staging unit matches a production PropertyUnit by composite key.
        // ============================================================
        var crossBatchResults = await DetectCrossBatchDuplicatesAsync(
            stagingUnits, buildingCodeByOriginalId, cancellationToken);
        results.AddRange(crossBatchResults);

        _logger.LogDebug(
            "Cross-batch property unit duplicates: {Count}", crossBatchResults.Count);

        _logger.LogInformation(
            "Property unit matching complete: {Scanned} units scanned, " +
            "{WithinBatch} within-batch + {CrossBatch} cross-batch = {Total} total matches",
            stagingUnits.Count, withinBatchResults.Count,
            crossBatchResults.Count, results.Count);

        return results;
    }

    // ==================== WITHIN-BATCH DETECTION ====================

    /// <summary>
    /// Detect within-batch duplicates: two staging units sharing the same
    /// BuildingCode + UnitIdentifier in the same import package.
    /// </summary>
    private List<PropertyMatchResult> DetectWithinBatchDuplicates(
        List<StagingPropertyUnit> stagingUnits,
        Dictionary<Guid, string> buildingCodeByOriginalId)
    {
        var results = new List<PropertyMatchResult>();
        var processedPairs = new HashSet<string>();

        // Group staging units by composite key
        var unitsByCompositeKey = stagingUnits
            .Where(u => buildingCodeByOriginalId.ContainsKey(u.OriginalBuildingId))
            .Select(u => new
            {
                Unit = u,
                BuildingCode = buildingCodeByOriginalId[u.OriginalBuildingId],
                CompositeKey = $"{buildingCodeByOriginalId[u.OriginalBuildingId]}|{u.UnitIdentifier}".ToUpperInvariant()
            })
            .GroupBy(x => x.CompositeKey)
            .Where(g => g.Count() > 1);

        foreach (var group in unitsByCompositeKey)
        {
            var items = group.ToList();
            // Generate pairwise matches within the group
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    var pairKey = string.Join("|",
                        new[] { items[i].Unit.OriginalEntityId, items[j].Unit.OriginalEntityId }
                            .OrderBy(id => id).Select(id => id.ToString()));

                    if (!processedPairs.Add(pairKey))
                        continue;

                    var compositeId = $"{items[i].BuildingCode}|{items[i].Unit.UnitIdentifier}";

                    results.Add(new PropertyMatchResult
                    {
                        StagingUnitId = items[i].Unit.Id,
                        StagingOriginalEntityId = items[i].Unit.OriginalEntityId,
                        MatchedEntityId = items[j].Unit.OriginalEntityId,
                        StagingUnitIdentifier = compositeId,
                        MatchedUnitIdentifier = $"{items[j].BuildingCode}|{items[j].Unit.UnitIdentifier}",
                        BuildingCode = items[i].BuildingCode,
                        UnitIdentifier = items[i].Unit.UnitIdentifier,
                        SimilarityScore = 100m,
                        ConfidenceLevel = "High",
                        IsWithinBatchMatch = true
                    });

                    _logger.LogDebug(
                        "Within-batch unit duplicate: {StagingA} ↔ {StagingB} (key: {Key})",
                        items[i].Unit.Id, items[j].Unit.Id, compositeId);
                }
            }
        }

        return results;
    }

    // ==================== CROSS-BATCH DETECTION ====================

    /// <summary>
    /// Detect cross-batch duplicates: a staging unit matching a production PropertyUnit
    /// by BuildingCode (17-digit) + UnitIdentifier.
    /// </summary>
    private async Task<List<PropertyMatchResult>> DetectCrossBatchDuplicatesAsync(
        List<StagingPropertyUnit> stagingUnits,
        Dictionary<Guid, string> buildingCodeByOriginalId,
        CancellationToken cancellationToken)
    {
        var results = new List<PropertyMatchResult>();
        var processedPairs = new HashSet<string>();

        foreach (var stagingUnit in stagingUnits)
        {
            // Resolve the 17-digit building code for this staging unit
            if (!buildingCodeByOriginalId.TryGetValue(
                    stagingUnit.OriginalBuildingId, out var buildingCode))
            {
                // Staging building has no BuildingId (17-digit code) — skip
                continue;
            }

            // Look up production PropertyUnit by BuildingCode + UnitIdentifier
            var productionMatch = await _propertyUnitRepository
                .GetByBuildingCodeAndUnitIdentifierAsync(
                    buildingCode, stagingUnit.UnitIdentifier, cancellationToken);

            if (productionMatch is null)
                continue;

            // Deduplicate: same staging-production pair
            var pairKey = $"{stagingUnit.OriginalEntityId}|{productionMatch.Id}";
            if (!processedPairs.Add(pairKey))
                continue;

            var compositeId = $"{buildingCode}|{stagingUnit.UnitIdentifier}";

            results.Add(new PropertyMatchResult
            {
                StagingUnitId = stagingUnit.Id,
                StagingOriginalEntityId = stagingUnit.OriginalEntityId,
                MatchedEntityId = productionMatch.Id,
                StagingUnitIdentifier = compositeId,
                MatchedUnitIdentifier = compositeId,
                BuildingCode = buildingCode,
                UnitIdentifier = stagingUnit.UnitIdentifier,
                SimilarityScore = 100m,
                ConfidenceLevel = "High",
                IsWithinBatchMatch = false
            });

            _logger.LogDebug(
                "Cross-batch unit duplicate: staging {StagingId} ↔ production {ProductionId} (key: {Key})",
                stagingUnit.Id, productionMatch.Id, compositeId);
        }

        return results;
    }
}
