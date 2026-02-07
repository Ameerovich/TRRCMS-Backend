using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Dtos;

/// <summary>
/// Full conflict detail DTO for the side-by-side review screen.
/// Used by UC-007 S03 (Property Duplicate Review) and UC-008 S03 (Person Duplicate Review).
/// Includes DataComparison, MatchingCriteria, and ReviewHistory for informed resolution.
/// </summary>
public class ConflictDetailDto
{
    // ==================== IDENTIFICATION ====================

    public Guid Id { get; set; }
    public string ConflictNumber { get; set; } = string.Empty;
    public string ConflictType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string ConflictDescription { get; set; } = string.Empty;

    // ==================== ENTITY PAIR ====================

    public Guid FirstEntityId { get; set; }
    public Guid SecondEntityId { get; set; }
    public string? FirstEntityIdentifier { get; set; }
    public string? SecondEntityIdentifier { get; set; }

    // ==================== SCORING ====================

    public decimal SimilarityScore { get; set; }
    public string ConfidenceLevel { get; set; } = string.Empty;

    /// <summary>
    /// JSON: matching criteria breakdown, e.g.
    /// {"national_id":"match","name_similarity":"95%","phone":"match"}
    /// </summary>
    public string? MatchingCriteria { get; set; }

    /// <summary>
    /// JSON: side-by-side field comparison for the review UI.
    /// Structure per entity type:
    ///   Person  → { fields: [{ field, first, second, match }], ... }
    ///   Building → { fields: [{ field, first, second, match }], ... }
    /// </summary>
    public string? DataComparison { get; set; }

    // ==================== STATUS ====================

    public string Status { get; set; } = string.Empty;
    public ConflictResolutionAction? ResolutionAction { get; set; }
    public string Priority { get; set; } = string.Empty;
    public bool IsEscalated { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsAutoDetected { get; set; }
    public bool IsAutoResolved { get; set; }
    public string? AutoResolutionRule { get; set; }

    // ==================== DATES & USERS ====================

    public DateTime DetectedDate { get; set; }
    public Guid? DetectedByUserId { get; set; }
    public DateTime? AssignedDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public DateTime? EscalatedDate { get; set; }
    public Guid? EscalatedByUserId { get; set; }

    // ==================== RESOLUTION DETAILS ====================

    public string? ResolutionReason { get; set; }
    public string? ResolutionNotes { get; set; }
    public Guid? MergedEntityId { get; set; }
    public Guid? DiscardedEntityId { get; set; }

    /// <summary>
    /// JSON: merge mapping audit trail showing which fields came from which entity.
    /// </summary>
    public string? MergeMapping { get; set; }

    // ==================== ESCALATION ====================

    public string? EscalationReason { get; set; }

    // ==================== REVIEW TRACKING ====================

    public int ReviewAttemptCount { get; set; }

    /// <summary>
    /// JSON array of previous review attempts.
    /// </summary>
    public string? ReviewHistory { get; set; }

    // ==================== CONTEXT ====================

    public Guid? ImportPackageId { get; set; }
    public int? TargetResolutionHours { get; set; }

    /// <summary>
    /// Elapsed time since detection (computed).
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }

    // ==================== AUDIT ====================

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}
