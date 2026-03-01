using MediatR;
using TRRCMS.Application.Conflicts.Queries.GetConflictQueue;

namespace TRRCMS.Application.Conflicts.Queries.GetEscalatedConflicts;

/// <summary>
/// Query for the senior review queue — returns only escalated conflicts
/// that are still pending resolution.
///
/// UC-007 S05a / UC-008 S05a: Escalated items awaiting supervisor decision.
/// </summary>
public class GetEscalatedConflictsQuery : IRequest<GetConflictQueueResponse>
{
    // ==================== FILTERS ====================

    /// <summary>
    /// Filter by conflict type: PersonDuplicate, PropertyDuplicate, ClaimConflict.
    /// Null returns all types.
    /// </summary>
    public string? ConflictType { get; set; }

    /// <summary>
    /// Filter by priority: Low, Normal, High, Critical.
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Scope to a specific import package.
    /// </summary>
    public Guid? ImportPackageId { get; set; }

    /// <summary>
    /// Show only overdue items.
    /// </summary>
    public bool? IsOverdue { get; set; }

    // ==================== PAGINATION ====================

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // ==================== SORTING ====================

    /// <summary>
    /// Sort field: EscalatedDate (default), Priority, SimilarityScore, DetectedDate.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort descending (default: true — newest escalations first).
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
