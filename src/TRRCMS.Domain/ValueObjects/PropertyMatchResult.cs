namespace TRRCMS.Domain.ValueObjects;

/// <summary>
/// Value object representing the result of comparing a staging PropertyUnit
/// against production or within-batch data for duplicate detection.
///
/// Detection criterion (UC-007, FR-D-6):
///   PropertyUnit duplicate = same BuildingCode (17-digit) + same UnitIdentifier.
///   Both fields must match exactly (case-insensitive for UnitIdentifier).
///
/// No building-level duplicate detection is applied — the same building may
/// legitimately appear across multiple surveys and claims.
///
/// Confidence: always High (exact composite key match).
/// Score: always 100 (deterministic key-based match, no fuzzy scoring).
/// </summary>
public class PropertyMatchResult
{
    // ==================== STAGING UNIT ====================

    /// <summary>Surrogate ID of the staging property unit.</summary>
    public Guid StagingUnitId { get; init; }

    /// <summary>OriginalEntityId of the staging property unit (from .uhc).</summary>
    public Guid StagingOriginalEntityId { get; init; }

    /// <summary>Human-readable composite key for the staging unit: "BuildingCode|UnitIdentifier".</summary>
    public string StagingUnitIdentifier { get; init; } = string.Empty;

    // ==================== MATCHED UNIT ====================

    /// <summary>
    /// ID of the matched entity.
    /// For cross-batch matches: production PropertyUnit.Id.
    /// For within-batch matches: StagingPropertyUnit.OriginalEntityId.
    /// </summary>
    public Guid MatchedEntityId { get; init; }

    /// <summary>Human-readable composite key for the matched unit: "BuildingCode|UnitIdentifier".</summary>
    public string MatchedUnitIdentifier { get; init; } = string.Empty;

    // ==================== MATCH DETAILS ====================

    /// <summary>The 17-digit building code shared by both units.</summary>
    public string BuildingCode { get; init; } = string.Empty;

    /// <summary>The unit identifier shared by both units.</summary>
    public string UnitIdentifier { get; init; } = string.Empty;

    /// <summary>Overall similarity score — always 100 for key-based matches.</summary>
    public decimal SimilarityScore { get; init; } = 100m;

    /// <summary>Confidence level — always High for exact composite key matches.</summary>
    public string ConfidenceLevel { get; init; } = "High";

    /// <summary>Whether this is a within-batch match (two staging records in same import).</summary>
    public bool IsWithinBatchMatch { get; init; }

    /// <summary>
    /// Build the matching criteria JSON for the ConflictResolution entity.
    /// </summary>
    public Dictionary<string, object> ToMatchingCriteria()
    {
        return new Dictionary<string, object>
        {
            ["building_code"] = BuildingCode,
            ["unit_identifier"] = UnitIdentifier,
            ["composite_key"] = $"{BuildingCode}|{UnitIdentifier}",
            ["match_type"] = IsWithinBatchMatch ? "within_batch" : "cross_batch",
            ["key_match"] = "exact",
            ["composite_score"] = SimilarityScore,
            ["confidence"] = ConfidenceLevel
        };
    }
}
