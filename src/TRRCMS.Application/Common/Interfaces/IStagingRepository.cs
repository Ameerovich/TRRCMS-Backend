using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Generic repository interface for all staging entities in the import pipeline.
/// Provides common data access operations shared by all 8 staging entity types
/// (Building, PropertyUnit, Person, Household, PersonPropertyRelation, Evidence, Claim, Survey).
/// 
/// Staging repositories are scoped to ImportPackage — most queries filter by ImportPackageId
/// since staging data is meaningful only within its parent package context.
/// 
/// Referenced in UC-003 (Import .uhc Package) Stages 2–4.
/// </summary>
/// <typeparam name="T">A concrete staging entity inheriting from <see cref="BaseStagingEntity"/>.</typeparam>
public interface IStagingRepository<T> where T : BaseStagingEntity
{
    // ==================== BASIC CRUD ====================

    /// <summary>
    /// Get a staging record by its surrogate ID.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a single staging record.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-add staging records (used during .uhc unpack — entire entity type in one batch).
    /// EF Core AddRangeAsync for optimal INSERT performance.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a staging record (validation status, approval, commit tracking).
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-update staging records (e.g. approve all valid records for commit).
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ==================== QUERY BY PACKAGE ====================

    /// <summary>
    /// Get all staging records belonging to a specific import package.
    /// Primary access pattern — staging data is always scoped to its package.
    /// </summary>
    Task<List<T>> GetByPackageIdAsync(Guid importPackageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a staging record by its original entity ID within a specific package.
    /// Used for intra-batch cross-referencing (e.g. StagingPropertyUnit → StagingBuilding).
    /// Unique constraint: (ImportPackageId, OriginalEntityId).
    /// </summary>
    Task<T?> GetByPackageAndOriginalIdAsync(
        Guid importPackageId,
        Guid originalEntityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of staging records in a package.
    /// </summary>
    Task<int> GetCountByPackageIdAsync(Guid importPackageId, CancellationToken cancellationToken = default);

    // ==================== QUERY BY VALIDATION STATUS ====================

    /// <summary>
    /// Get staging records by validation status within a package.
    /// Used by validation pipeline (get Pending), review UI (get Invalid/Warning),
    /// and commit pipeline (get Valid + approved).
    /// </summary>
    Task<List<T>> GetByPackageAndStatusAsync(
        Guid importPackageId,
        StagingValidationStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get counts grouped by validation status for a package.
    /// Returns a dictionary: { Pending: N, Valid: N, Invalid: N, Warning: N, Skipped: N }.
    /// Used for the package review dashboard.
    /// </summary>
    Task<Dictionary<StagingValidationStatus, int>> GetStatusCountsByPackageAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default);

    // ==================== COMMIT QUERIES ====================

    /// <summary>
    /// Get all records approved for commit within a package.
    /// Used by the commit pipeline (Phase 2D) to know what to write to production.
    /// </summary>
    Task<List<T>> GetApprovedForCommitAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all records that have been committed (have a CommittedEntityId).
    /// Used for post-commit verification and traceability reporting.
    /// </summary>
    Task<List<T>> GetCommittedAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default);

    // ==================== BULK OPERATIONS ====================

    /// <summary>
    /// Delete all staging records for a package.
    /// Used when cancelling an import or cleaning up after successful commit.
    /// Note: FK CASCADE on ImportPackage covers this too, but explicit delete
    /// is useful for partial cleanup scenarios.
    /// </summary>
    Task<int> DeleteByPackageIdAsync(Guid importPackageId, CancellationToken cancellationToken = default);

    // ==================== QUERYABLE ACCESS ====================

    /// <summary>
    /// Get IQueryable for custom queries (pagination, sorting, complex filters).
    /// Caller must materialize results with ToListAsync/CountAsync.
    /// </summary>
    IQueryable<T> GetQueryable();
}
