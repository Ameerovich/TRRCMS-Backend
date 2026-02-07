namespace TRRCMS.Domain.ValueObjects;

/// <summary>
/// Value object representing the result of comparing a staging person
/// against a production or staging person for duplicate detection.
///
/// Scoring (FR-D-5, §12.2.4):
///   - National ID exact match:       100 points (auto-flag as high confidence)
///   - Phone number exact match:       +30 points
///   - Arabic name Levenshtein:         0–40 points
///   - Year of birth exact match:      +15 points
///   - Gender exact match:             +15 points
///   Maximum composite score:          100 (capped)
///
/// Thresholds:
///   - Score ≥ 90 → High confidence   → auto-flag for review
///   - Score 70–89 → Medium confidence → manual review
///   - Score &lt; 70 → below threshold  → not a duplicate
/// </summary>
public class PersonMatchResult
{
    /// <summary>ID of the staging person being checked.</summary>
    public Guid StagingPersonId { get; init; }

    /// <summary>Original entity ID of the staging person (from .uhc).</summary>
    public Guid StagingOriginalEntityId { get; init; }

    /// <summary>
    /// ID of the matched entity. Could be a production Person.Id
    /// or another staging person's Id (within-batch duplicate).
    /// </summary>
    public Guid MatchedEntityId { get; init; }

    /// <summary>Human-readable identifier for the staging person.</summary>
    public string StagingPersonIdentifier { get; init; } = string.Empty;

    /// <summary>Human-readable identifier for the matched person.</summary>
    public string MatchedPersonIdentifier { get; init; } = string.Empty;

    /// <summary>Whether the match is against a production entity or within-batch staging entity.</summary>
    public bool IsWithinBatchMatch { get; init; }

    /// <summary>Overall similarity score (0–100).</summary>
    public decimal SimilarityScore { get; init; }

    /// <summary>Confidence level derived from score thresholds.</summary>
    public string ConfidenceLevel => SimilarityScore >= 90 ? "High"
                                   : SimilarityScore >= 70 ? "Medium"
                                   : "Low";

    // ==================== INDIVIDUAL CRITERIA ====================

    public bool NationalIdMatched { get; init; }
    public bool PhoneMatched { get; init; }
    public decimal NameSimilarityScore { get; init; }
    public bool YearOfBirthMatched { get; init; }
    public bool GenderMatched { get; init; }

    /// <summary>
    /// Build the matching criteria JSON for the ConflictResolution entity.
    /// </summary>
    public Dictionary<string, object> ToMatchingCriteria()
    {
        var criteria = new Dictionary<string, object>
        {
            ["national_id"] = NationalIdMatched ? "exact_match" : "no_match",
            ["phone"] = PhoneMatched ? "exact_match" : "no_match",
            ["name_similarity"] = $"{NameSimilarityScore:F1}%",
            ["year_of_birth"] = YearOfBirthMatched ? "exact_match" : "no_match",
            ["gender"] = GenderMatched ? "exact_match" : "no_match",
            ["composite_score"] = SimilarityScore,
            ["confidence"] = ConfidenceLevel,
            ["is_within_batch"] = IsWithinBatchMatch
        };
        return criteria;
    }
}
