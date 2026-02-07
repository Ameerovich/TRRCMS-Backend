namespace TRRCMS.Domain.ValueObjects;

/// <summary>
/// Value object representing the result of comparing a staging building/property
/// against production data for duplicate detection.
///
/// Matching criteria (FR-D-6, §12.2.5):
///   - BuildingId exact match (17-digit composite code):  primary detection
///   - Unit code match within matched building:            unit-level duplicate
///   - Spatial proximity (lat/lng distance &lt; 50m)
///     + similar attributes (type, status):                fuzzy/spatial detection
///
/// Thresholds:
///   - BuildingId exact match → High confidence
///   - Spatial + attributes match → Medium confidence
/// </summary>
public class PropertyMatchResult
{
    /// <summary>ID of the staging building being checked.</summary>
    public Guid StagingBuildingId { get; init; }

    /// <summary>Original entity ID of the staging building (from .uhc).</summary>
    public Guid StagingOriginalEntityId { get; init; }

    /// <summary>
    /// ID of the matched production building.
    /// </summary>
    public Guid MatchedBuildingId { get; init; }

    /// <summary>Human-readable identifier for the staging building.</summary>
    public string StagingBuildingIdentifier { get; init; } = string.Empty;

    /// <summary>Human-readable identifier for the matched building.</summary>
    public string MatchedBuildingIdentifier { get; init; } = string.Empty;

    /// <summary>Overall similarity score (0–100).</summary>
    public decimal SimilarityScore { get; init; }

    /// <summary>Confidence level derived from match type.</summary>
    public string ConfidenceLevel { get; init; } = "Medium";

    // ==================== INDIVIDUAL CRITERIA ====================

    /// <summary>Whether the 17-digit BuildingId matched exactly.</summary>
    public bool BuildingIdMatched { get; init; }

    /// <summary>Distance in meters between staging and production building coordinates.</summary>
    public double? DistanceMeters { get; init; }

    /// <summary>Whether building types match.</summary>
    public bool BuildingTypeMatched { get; init; }

    /// <summary>Whether spatial proximity threshold (50m) was met.</summary>
    public bool WithinSpatialThreshold { get; init; }

    /// <summary>List of unit-level duplicates within this building pair.</summary>
    public List<UnitMatchDetail> UnitMatches { get; init; } = new();

    /// <summary>
    /// Build the matching criteria JSON for the ConflictResolution entity.
    /// </summary>
    public Dictionary<string, object> ToMatchingCriteria()
    {
        var criteria = new Dictionary<string, object>
        {
            ["building_id_match"] = BuildingIdMatched ? "exact_match" : "no_match",
            ["distance_meters"] = DistanceMeters?.ToString("F1") ?? "N/A",
            ["building_type_match"] = BuildingTypeMatched ? "match" : "no_match",
            ["within_spatial_threshold"] = WithinSpatialThreshold,
            ["unit_duplicates_count"] = UnitMatches.Count,
            ["composite_score"] = SimilarityScore,
            ["confidence"] = ConfidenceLevel
        };
        return criteria;
    }
}

/// <summary>
/// Detail of a unit-level match within a building duplicate pair.
/// </summary>
public class UnitMatchDetail
{
    public Guid StagingUnitId { get; init; }
    public Guid MatchedUnitId { get; init; }
    public string UnitIdentifier { get; init; } = string.Empty;
}
