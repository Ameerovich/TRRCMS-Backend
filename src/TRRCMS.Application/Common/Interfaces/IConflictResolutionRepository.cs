using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for ConflictResolution entity.
/// Provides data access for duplicate detection and conflict resolution workflow.
/// 
/// Key responsibilities:
/// - Conflict queue management (pending review → resolved/ignored)
/// - Entity-pair lookups (prevent duplicate conflict records)
/// - Assignment and escalation tracking
/// - Package-scoped conflict queries
/// </summary>
public interface IConflictResolutionRepository
{
    /// <summary>
    /// Get conflict by surrogate ID.
    /// </summary>
    Task<ConflictResolution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all conflicts (excluding soft-deleted).
    /// </summary>
    Task<List<ConflictResolution>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new conflict record.
    /// </summary>
    Task AddAsync(ConflictResolution conflict, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-add conflict records (batch detection results).
    /// </summary>
    Task AddRangeAsync(IEnumerable<ConflictResolution> conflicts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing conflict record.
    /// </summary>
    Task UpdateAsync(ConflictResolution conflict, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard-delete a batch of conflict records.
    /// Used when re-running duplicate detection to remove superseded PendingReview conflicts
    /// so fresh detection can create new ones without being blocked by stale records.
    /// </summary>
    Task RemoveRangeAsync(IEnumerable<ConflictResolution> conflicts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// Get all conflicts triggered by a specific import package.
    /// </summary>
    Task<List<ConflictResolution>> GetByPackageIdAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of conflicts for a package.
    /// </summary>
    Task<int> GetCountByPackageIdAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of unresolved conflicts for a package.
    /// Used to determine if a package can proceed to commit.
    /// </summary>
    Task<int> GetUnresolvedCountByPackageIdAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if all conflicts for a package have been resolved.
    /// Gate check before allowing commit to production.
    /// </summary>
    Task<bool> AreAllResolvedForPackageAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all resolved merge conflicts for a package, filtered by entity type.
    /// Used during commit to populate ID map redirects for discarded entities
    /// so that child staging entities (e.g. Claims referencing merged PropertyUnits)
    /// resolve their FK references correctly.
    /// </summary>
    Task<List<ConflictResolution>> GetResolvedMergesForPackageAsync(
        Guid importPackageId,
        string entityType,
        CancellationToken cancellationToken = default);



    /// <summary>
    /// Get conflicts by status string (PendingReview, Resolved, Ignored).
    /// Primary queue query for the conflict resolution UI.
    /// </summary>
    Task<List<ConflictResolution>> GetByStatusAsync(
        string status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all pending (unresolved) conflicts across all packages.
    /// Used for the global conflict queue.
    /// </summary>
    Task<List<ConflictResolution>> GetPendingAsync(
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Get conflict by the pair of entity IDs (order-independent).
    /// Used to check if a conflict already exists between two entities
    /// before creating a new conflict record.
    /// </summary>
    Task<ConflictResolution?> GetByEntityPairAsync(
        Guid firstEntityId,
        Guid secondEntityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a previous KeepSeparate (KeepBoth) resolution exists for the given entity pair.
    /// Per UC rules: keep-separate decisions prevent the same pair from being
    /// re-surfaced as a duplicate unless detection rules or keys change.
    /// Used by DuplicateDetectionService to suppress re-flagging of already-reviewed pairs.
    /// </summary>
    /// <param name="firstEntityId">First entity in the pair (order-independent).</param>
    /// <param name="secondEntityId">Second entity in the pair (order-independent).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a resolved KeepBoth conflict exists for this pair.</returns>
    Task<bool> HasKeepSeparateDecisionAsync(
        Guid firstEntityId,
        Guid secondEntityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all conflicts involving a specific entity (as either first or second).
    /// Used to show all conflicts for a given person/building/property in the review UI.
    /// </summary>
    Task<List<ConflictResolution>> GetByEntityIdAsync(
        Guid entityId,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Get conflicts assigned to a specific user.
    /// </summary>
    Task<List<ConflictResolution>> GetByAssignedUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get escalated conflicts.
    /// </summary>
    Task<List<ConflictResolution>> GetEscalatedAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue conflicts (past SLA target).
    /// </summary>
    Task<List<ConflictResolution>> GetOverdueAsync(
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Get conflicts by conflict type (PersonDuplicate, PropertyDuplicate, ClaimConflict).
    /// </summary>
    Task<List<ConflictResolution>> GetByConflictTypeAsync(
        string conflictType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get conflicts by conflict type and status.
    /// Filtered queue for the conflict resolution UI.
    /// </summary>
    Task<List<ConflictResolution>> GetByConflictTypeAndStatusAsync(
        string conflictType,
        string status,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Search conflicts with filters and pagination.
    /// Supports the conflict management list view.
    /// </summary>
    Task<(List<ConflictResolution> Conflicts, int TotalCount)> SearchAsync(
        Guid? importPackageId = null,
        string? conflictType = null,
        string? entityType = null,
        string? status = null,
        string? priority = null,
        Guid? assignedToUserId = null,
        bool? isEscalated = null,
        bool? isOverdue = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = true,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Get count of conflicts grouped by status.
    /// </summary>
    Task<Dictionary<string, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of conflicts grouped by conflict type.
    /// </summary>
    Task<Dictionary<string, int>> GetTypeCountsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total conflict count (excluding soft-deleted).
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if conflict exists by surrogate ID.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);


    /// <summary>
    /// Get IQueryable for custom queries.
    /// </summary>
    IQueryable<ConflictResolution> GetQueryable();
}