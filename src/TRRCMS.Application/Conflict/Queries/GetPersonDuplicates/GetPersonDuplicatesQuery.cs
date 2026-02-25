using MediatR;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Application.Conflicts.Queries.GetConflictQueue;

namespace TRRCMS.Application.Conflicts.Queries.GetPersonDuplicates;

/// <summary>
/// Query to retrieve paginated person duplicate groups for the review queue.
///
/// Filters the conflict queue to PersonDuplicate conflict type only,
/// grouping by national_id exact matches.
/// </summary>
public class GetPersonDuplicatesQuery : IRequest<GetConflictQueueResponse>
{
    // ==================== FILTERS ====================

    /// <summary>
    /// Filter by status: PendingReview, Resolved, Ignored.
    /// Defaults to PendingReview for the active queue.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by priority: Low, Normal, High, Critical.
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Scope to a specific import package.
    /// </summary>
    public Guid? ImportPackageId { get; set; }

    /// <summary>
    /// Filter by assigned reviewer.
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// Show only escalated items.
    /// </summary>
    public bool? IsEscalated { get; set; }

    /// <summary>
    /// Show only overdue items.
    /// </summary>
    public bool? IsOverdue { get; set; }

    // ==================== PAGINATION ====================

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // ==================== SORTING ====================

    /// <summary>
    /// Sort field: DetectedDate, SimilarityScore, Priority (default: DetectedDate).
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort descending (default: true — newest first).
    /// </summary>
    public bool SortDescending { get; set; } = true;
}
