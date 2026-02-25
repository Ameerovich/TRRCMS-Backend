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
/// 
/// Referenced in UC-003 Stage 3 (Conflict Detection) and UC-004 (Resolve Conflicts).
/// </summary>
public interface IConflictResolutionRepository
{
    // ==================== BASIC CRUD ====================

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
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ==================== QUERY BY PACKAGE ====================

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

    // ==================== QUERY BY STATUS ====================

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

    // ==================== QUERY BY ENTITY PAIR ====================

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
    /// Get all conflicts involving a specific entity (as either first or second).
    /// Used to show all conflicts for a given person/building/property in the review UI.
    /// </summary>
    Task<List<ConflictResolution>> GetByEntityIdAsync(
        Guid entityId,
        CancellationToken cancellationToken = default);

    // ==================== QUERY BY ASSIGNMENT ====================

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

    // ==================== QUERY BY TYPE ====================

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

    // ==================== SEARCH WITH PAGINATION ====================

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

    // ==================== AGGREGATE QUERIES ====================

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

    // ==================== QUERYABLE ACCESS ====================

    /// <summary>
    /// Get IQueryable for custom queries.
    /// </summary>
    IQueryable<ConflictResolution> GetQueryable();
}
