using MediatR;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictQueue;

/// <summary>
/// Query to retrieve the conflict resolution queue with filtering and pagination.
/// </summary>
public class GetConflictQueueQuery : IRequest<GetConflictQueueResponse>
{
    // ==================== FILTERS ====================

    /// <summary>
    /// Filter by conflict type: PersonDuplicate, PropertyDuplicate, ClaimConflict.
    /// Null returns all types.
    /// </summary>
    public string? ConflictType { get; set; }

    /// <summary>
    /// Filter by status: PendingReview, Resolved, Ignored.
    /// Null returns all statuses.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by priority: Low, Normal, High, Critical.
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Filter conflicts belonging to a specific import package.
    /// </summary>
    public Guid? ImportPackageId { get; set; }

    /// <summary>
    /// Filter by assigned user.
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// Filter escalated only.
    /// </summary>
    public bool? IsEscalated { get; set; }

    /// <summary>
    /// Filter overdue only.
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

/// <summary>
/// Paginated response wrapper for the conflict queue.
/// </summary>
public class GetConflictQueueResponse
{
    public List<ConflictDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
