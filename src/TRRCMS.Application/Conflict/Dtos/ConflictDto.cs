using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Dtos;

/// <summary>
/// Lightweight DTO for conflict queue listing.
/// Maps to each row in the conflict queue grid.
/// Excludes heavy fields (DataComparison, MergeMapping) for performance.
/// </summary>
public class ConflictDto
{
    // ==================== IDENTIFICATION ====================

    public Guid Id { get; set; }
    public string ConflictNumber { get; set; } = string.Empty;

    // ==================== CLASSIFICATION ====================

    /// <summary>
    /// PersonDuplicate | PropertyDuplicate | ClaimConflict
    /// </summary>
    public string ConflictType { get; set; } = string.Empty;

    /// <summary>
    /// Person | Building | PropertyUnit
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    // ==================== ENTITY PAIR ====================

    public Guid FirstEntityId { get; set; }
    public Guid SecondEntityId { get; set; }
    public string? FirstEntityIdentifier { get; set; }
    public string? SecondEntityIdentifier { get; set; }

    // ==================== SCORING ====================

    /// <summary>
    /// 0–100 similarity score
    /// </summary>
    public decimal SimilarityScore { get; set; }

    /// <summary>
    /// Low | Medium | High
    /// </summary>
    public string ConfidenceLevel { get; set; } = string.Empty;

    // ==================== STATUS ====================

    /// <summary>
    /// PendingReview | Resolved | Ignored
    /// </summary>
    public string Status { get; set; } = string.Empty;

    public ConflictResolutionAction? ResolutionAction { get; set; }
    public string Priority { get; set; } = string.Empty;
    public bool IsEscalated { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsAutoDetected { get; set; }
    public bool IsAutoResolved { get; set; }

    // ==================== DATES ====================

    public DateTime DetectedDate { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }

    // ==================== CONTEXT ====================

    public Guid? ImportPackageId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? ResolvedByUserId { get; set; }
}
