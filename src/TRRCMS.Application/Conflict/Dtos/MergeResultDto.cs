namespace TRRCMS.Application.Conflicts.Dtos;

/// <summary>
/// Result of a merge operation performed by PersonMergeService or PropertyMergeService.
/// Captures the outcome including master entity, discarded entity, and FK propagation counts
/// for audit trail and commit report generation.
/// </summary>
public class MergeResultDto
{
    /// <summary>
    /// Whether the merge completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// ID of the surviving (master) entity after merge.
    /// </summary>
    public Guid MasterEntityId { get; set; }

    /// <summary>
    /// ID of the discarded (soft-deleted or deactivated) entity.
    /// </summary>
    public Guid DiscardedEntityId { get; set; }

    /// <summary>
    /// JSON describing which fields came from which source entity.
    /// Example: {"FirstNameArabic":"first","MobileNumber":"second","NationalId":"first"}
    /// </summary>
    public string MergeMappingJson { get; set; } = string.Empty;

    /// <summary>
    /// Number of FK references re-pointed from discarded → master entity.
    /// </summary>
    public int ReferencesUpdated { get; set; }

    /// <summary>
    /// Breakdown of updated references by entity type.
    /// Example: {"PersonPropertyRelation":3,"Claim":1,"Household":2}
    /// </summary>
    public Dictionary<string, int> ReferencesByType { get; set; } = new();

    /// <summary>
    /// Fields where both entities had different non-null values.
    /// The master value was kept; the discarded value is recorded for audit.
    /// </summary>
    public Dictionary<string, FieldConflictInfo> ConflictingFields { get; set; } = new();

    /// <summary>
    /// Error message if merge failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Records a field-level conflict where both entities had different non-null values.
/// </summary>
public record FieldConflictInfo(string? MasterValue, string? DiscardedValue, string KeptFrom);
