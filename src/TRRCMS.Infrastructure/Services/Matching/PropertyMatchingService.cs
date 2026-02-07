using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Infrastructure.Services.Matching;

/// <summary>
/// Compares staging buildings and property units against production data
/// to detect duplicate properties.
///
/// Matching strategy (FR-D-6):
///   1. BuildingId exact match (17-digit composite code) → High confidence
///   2. Spatial proximity (lat/lng within 50m) + same BuildingType → Medium confidence
///   3. Unit-level: UnitIdentifier exact match within a matched building pair
///
/// UC-007 (Resolve Duplicate Properties).
/// </summary>
public class PropertyMatchingService
{
    private const int SpatialThresholdMeters = 50;
    private const double MetersPerDegreeLat = 111_320.0; // approximate at equator

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
    /// Run property matching for all valid/warning staging buildings in a package.
    /// </summary>
    /// <param name="stagingBuildings">Staging buildings to check (already filtered to Valid/Warning).</param>
    /// <param name="stagingUnits">Staging property units for unit-level matching.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of property match results above threshold.</returns>
    public async Task<List<PropertyMatchResult>> DetectDuplicatesAsync(
        List<StagingBuilding> stagingBuildings,
        List<StagingPropertyUnit> stagingUnits,
        CancellationToken cancellationToken = default)
    {
        var results = new List<PropertyMatchResult>();

        if (stagingBuildings.Count == 0)
            return results;

        // ============================================================
        // Phase 1: BuildingId exact matches (production)
        // ============================================================
        var matchedStagingIds = new HashSet<Guid>();

        foreach (var staging in stagingBuildings.Where(b => !string.IsNullOrWhiteSpace(b.BuildingId)))
        {
            var productionMatch = await _buildingRepository
                .GetByBuildingIdAsync(staging.BuildingId!, cancellationToken);

            if (productionMatch != null)
            {
                var unitMatches = await FindUnitMatchesAsync(
                    staging.OriginalEntityId, stagingUnits,
                    productionMatch.Id, cancellationToken);

                results.Add(new PropertyMatchResult
                {
                    StagingBuildingId = staging.Id,
                    StagingOriginalEntityId = staging.OriginalEntityId,
                    MatchedBuildingId = productionMatch.Id,
                    StagingBuildingIdentifier = staging.BuildingId!,
                    MatchedBuildingIdentifier = productionMatch.BuildingId,
                    SimilarityScore = 100m,
                    ConfidenceLevel = "High",
                    BuildingIdMatched = true,
                    DistanceMeters = ComputeHaversineDistance(
                        staging.Latitude, staging.Longitude,
                        productionMatch.Latitude, productionMatch.Longitude),
                    BuildingTypeMatched = staging.BuildingType == productionMatch.BuildingType,
                    WithinSpatialThreshold = true,
                    UnitMatches = unitMatches
                });

                matchedStagingIds.Add(staging.Id);

                _logger.LogDebug(
                    "BuildingId exact match: staging {StagingId} ({BuildingId}) ↔ production {ProductionId}",
                    staging.Id, staging.BuildingId, productionMatch.Id);
            }
        }

        // ============================================================
        // Phase 2: Spatial proximity matches (production)
        // ============================================================
        var unmatchedWithCoords = stagingBuildings
            .Where(b => !matchedStagingIds.Contains(b.Id) &&
                        b.Latitude.HasValue && b.Longitude.HasValue)
            .ToList();

        foreach (var staging in unmatchedWithCoords)
        {
            var nearbyBuildings = await _buildingRepository
                .GetBuildingsWithinRadiusAsync(
                    staging.Latitude!.Value,
                    staging.Longitude!.Value,
                    SpatialThresholdMeters,
                    cancellationToken);

            foreach (var nearby in nearbyBuildings)
            {
                var distance = ComputeHaversineDistance(
                    staging.Latitude, staging.Longitude,
                    nearby.Latitude, nearby.Longitude);

                bool typeMatched = staging.BuildingType == nearby.BuildingType;

                // Require at least BuildingType match for spatial candidates
                // to reduce false positives in dense urban areas
                if (!typeMatched)
                    continue;

                var score = ComputeSpatialScore(distance, typeMatched);

                if (score >= 70m)
                {
                    var unitMatches = await FindUnitMatchesAsync(
                        staging.OriginalEntityId, stagingUnits,
                        nearby.Id, cancellationToken);

                    results.Add(new PropertyMatchResult
                    {
                        StagingBuildingId = staging.Id,
                        StagingOriginalEntityId = staging.OriginalEntityId,
                        MatchedBuildingId = nearby.Id,
                        StagingBuildingIdentifier = staging.BuildingId ?? BuildBuildingIdentifier(staging),
                        MatchedBuildingIdentifier = nearby.BuildingId,
                        SimilarityScore = score,
                        ConfidenceLevel = "Medium",
                        BuildingIdMatched = false,
                        DistanceMeters = distance,
                        BuildingTypeMatched = typeMatched,
                        WithinSpatialThreshold = distance.HasValue && distance.Value <= SpatialThresholdMeters,
                        UnitMatches = unitMatches
                    });

                    _logger.LogDebug(
                        "Spatial match ({Score}, {Distance}m): staging {StagingId} ↔ production {ProductionId}",
                        score, distance?.ToString("F1") ?? "N/A", staging.Id, nearby.Id);
                }
            }
        }

        // Deduplicate: same staging-production pair, keep highest score
        results = results
            .GroupBy(r => new { r.StagingBuildingId, r.MatchedBuildingId })
            .Select(g => g.OrderByDescending(r => r.SimilarityScore).First())
            .ToList();

        _logger.LogInformation(
            "Property matching complete: {Scanned} buildings scanned, {Matches} matches found",
            stagingBuildings.Count, results.Count);

        return results;
    }

    // ==================== UNIT-LEVEL MATCHING ====================

    /// <summary>
    /// Find unit-level duplicates within a matched building pair.
    /// Compares staging units (by OriginalBuildingId) against production units.
    /// </summary>
    private async Task<List<UnitMatchDetail>> FindUnitMatchesAsync(
        Guid stagingOriginalBuildingId,
        List<StagingPropertyUnit> allStagingUnits,
        Guid productionBuildingId,
        CancellationToken cancellationToken)
    {
        var unitMatches = new List<UnitMatchDetail>();

        var stagingUnitsForBuilding = allStagingUnits
            .Where(u => u.OriginalBuildingId == stagingOriginalBuildingId)
            .ToList();

        if (stagingUnitsForBuilding.Count == 0)
            return unitMatches;

        var productionUnits = await _propertyUnitRepository
            .GetByBuildingIdAsync(productionBuildingId, cancellationToken);

        foreach (var stagingUnit in stagingUnitsForBuilding)
        {
            var matchedUnit = productionUnits
                .FirstOrDefault(pu => string.Equals(
                    pu.UnitIdentifier, stagingUnit.UnitIdentifier,
                    StringComparison.OrdinalIgnoreCase));

            if (matchedUnit != null)
            {
                unitMatches.Add(new UnitMatchDetail
                {
                    StagingUnitId = stagingUnit.Id,
                    MatchedUnitId = matchedUnit.Id,
                    UnitIdentifier = stagingUnit.UnitIdentifier
                });
            }
        }

        return unitMatches;
    }

    // ==================== SCORING ====================

    /// <summary>
    /// Compute similarity score for spatial + attribute matching.
    /// Base score from proximity (80 max) + type match bonus (20).
    /// </summary>
    private static decimal ComputeSpatialScore(double? distanceMeters, bool typeMatched)
    {
        if (!distanceMeters.HasValue)
            return 0m;

        // Proximity score: 80 at 0m, linearly decreasing to 0 at threshold
        decimal proximityScore = distanceMeters.Value <= SpatialThresholdMeters
            ? 80m * (1m - (decimal)distanceMeters.Value / SpatialThresholdMeters)
            : 0m;

        // Type match bonus
        decimal typeBonus = typeMatched ? 20m : 0m;

        return Math.Round(proximityScore + typeBonus, 1);
    }

    // ==================== DISTANCE CALCULATION ====================

    /// <summary>
    /// Compute approximate distance between two lat/lng points using Haversine formula.
    /// Returns null if either point has no coordinates.
    /// </summary>
    internal static double? ComputeHaversineDistance(
        decimal? lat1, decimal? lng1, decimal? lat2, decimal? lng2)
    {
        if (!lat1.HasValue || !lng1.HasValue || !lat2.HasValue || !lng2.HasValue)
            return null;

        const double earthRadiusMeters = 6_371_000.0;

        double dLat = DegreesToRadians((double)(lat2.Value - lat1.Value));
        double dLng = DegreesToRadians((double)(lng2.Value - lng1.Value));

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(DegreesToRadians((double)lat1.Value)) *
                   Math.Cos(DegreesToRadians((double)lat2.Value)) *
                   Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static string BuildBuildingIdentifier(StagingBuilding staging)
    {
        return $"{staging.GovernorateCode}-{staging.DistrictCode}-" +
               $"{staging.SubDistrictCode}-{staging.CommunityCode}-" +
               $"{staging.NeighborhoodCode}-{staging.BuildingNumber}";
    }
}
