using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for ImportPackage entity.
/// Provides data access for the .uhc import pipeline package lifecycle.
/// 
/// Key responsibilities:
/// - Idempotency enforcement (unique PackageId from manifest)
/// - Status-based workflow queries (Pending → Validating → ... → Completed)
/// - Package search and filtering for the import management UI
/// </summary>
public interface IImportPackageRepository
{
    /// <summary>
    /// Get package by surrogate ID.
    /// </summary>
    Task<ImportPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all packages (with optional soft-delete filter).
    /// </summary>
    Task<List<ImportPackage>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new import package.
    /// </summary>
    Task AddAsync(ImportPackage package, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing import package.
    /// </summary>
    Task UpdateAsync(ImportPackage package, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// Get package by its manifest PackageId (the GUID from the .uhc manifest).
    /// Used for idempotency check: if a package with this PackageId already exists,
    /// the import is rejected as a duplicate.
    /// Unique index: IX_ImportPackages_PackageId.
    /// </summary>
    Task<ImportPackage?> GetByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a package with the given manifest PackageId already exists.
    /// Fast existence check without loading the full entity.
    /// </summary>
    Task<bool> ExistsByPackageIdAsync(Guid packageId, CancellationToken cancellationToken = default);


    /// <summary>
    /// Get all packages with a specific import status.
    /// Primary workflow query — each pipeline stage pulls packages in its target status.
    /// </summary>
    Task<List<ImportPackage>> GetByStatusAsync(
        ImportStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get packages with any of the specified statuses.
    /// Used for dashboard views (e.g. "all active imports" = Pending + Validating + Staging + ReviewingConflicts).
    /// </summary>
    Task<List<ImportPackage>> GetByStatusesAsync(
        IEnumerable<ImportStatus> statuses,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of packages grouped by status.
    /// Used for the import dashboard summary tiles.
    /// </summary>
    Task<Dictionary<ImportStatus, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Get packages exported by a specific field collector.
    /// </summary>
    Task<List<ImportPackage>> GetByExportedByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get packages imported by a specific desktop user.
    /// </summary>
    Task<List<ImportPackage>> GetByImportedByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Get packages imported within a date range.
    /// </summary>
    Task<List<ImportPackage>> GetByImportedDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Search packages with filters and pagination.
    /// Supports the import management list view with sorting and filtering.
    /// </summary>
    Task<(List<ImportPackage> Packages, int TotalCount)> SearchAsync(
        ImportStatus? status = null,
        Guid? exportedByUserId = null,
        Guid? importedByUserId = null,
        DateTime? importedAfter = null,
        DateTime? importedBefore = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = true,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Get packages that have unresolved conflicts.
    /// Used for the conflict resolution queue.
    /// </summary>
    Task<List<ImportPackage>> GetWithUnresolvedConflictsAsync(
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Get total count of packages (excluding soft-deleted).
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if package exists by surrogate ID.
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get sum of content counts from completed packages (SurveyCount, BuildingCount, PersonCount).
    /// Used for dashboard summary tiles.
    /// </summary>
    Task<(int Surveys, int Buildings, int Persons)> GetCompletedContentTotalsAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// Get IQueryable for custom queries.
    /// </summary>
    IQueryable<ImportPackage> GetQueryable();


    /// <summary>
    /// Get monthly creation counts for time-series trends.
    /// </summary>
    Task<List<(int Year, int Month, int Count)>> GetMonthlyCreationCountsAsync(
        DateTime? from = null, DateTime? to = null,
        CancellationToken cancellationToken = default);
}
