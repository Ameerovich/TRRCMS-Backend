using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ImportPackage entity.
/// Handles the full lifecycle of .uhc import packages from upload to completion.
/// </summary>
public class ImportPackageRepository : IImportPackageRepository
{
    private readonly ApplicationDbContext _context;

    public ImportPackageRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ==================== BASIC CRUD ====================

    public async Task<ImportPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<List<ImportPackage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ImportPackage package, CancellationToken cancellationToken = default)
    {
        await _context.ImportPackages.AddAsync(package, cancellationToken);
    }

    public Task UpdateAsync(ImportPackage package, CancellationToken cancellationToken = default)
    {
        _context.ImportPackages.Update(package);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // ==================== IDEMPOTENCY ====================

    public async Task<ImportPackage?> GetByPackageIdAsync(
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .FirstOrDefaultAsync(p => p.PackageId == packageId, cancellationToken);
    }

    public async Task<bool> ExistsByPackageIdAsync(
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .AnyAsync(p => p.PackageId == packageId, cancellationToken);
    }

    // ==================== STATUS WORKFLOW ====================

    public async Task<List<ImportPackage>> GetByStatusAsync(
        ImportStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .Where(p => p.Status == status && !p.IsDeleted)
            .OrderBy(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ImportPackage>> GetByStatusesAsync(
        IEnumerable<ImportStatus> statuses,
        CancellationToken cancellationToken = default)
    {
        var statusList = statuses.ToList();
        return await _context.ImportPackages
            .Where(p => statusList.Contains(p.Status) && !p.IsDeleted)
            .OrderBy(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<ImportStatus, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .Where(p => !p.IsDeleted)
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    // ==================== QUERY BY USER ====================

    public async Task<List<ImportPackage>> GetByExportedByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .Where(p => p.ExportedByUserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.PackageExportedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ImportPackage>> GetByImportedByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .Where(p => p.ImportedByUserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.ImportedDate)
            .ToListAsync(cancellationToken);
    }

    // ==================== QUERY BY DATE ====================

    public async Task<List<ImportPackage>> GetByImportedDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .Where(p => p.ImportedDate.HasValue
                && p.ImportedDate.Value >= startDate
                && p.ImportedDate.Value <= endDate
                && !p.IsDeleted)
            .OrderBy(p => p.ImportedDate)
            .ToListAsync(cancellationToken);
    }

    // ==================== SEARCH WITH PAGINATION ====================

    public async Task<(List<ImportPackage> Packages, int TotalCount)> SearchAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = _context.ImportPackages
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (exportedByUserId.HasValue)
            query = query.Where(p => p.ExportedByUserId == exportedByUserId.Value);

        if (importedByUserId.HasValue)
            query = query.Where(p => p.ImportedByUserId == importedByUserId.Value);

        if (importedAfter.HasValue)
            query = query.Where(p => p.ImportedDate.HasValue && p.ImportedDate.Value >= importedAfter.Value);

        if (importedBefore.HasValue)
            query = query.Where(p => p.ImportedDate.HasValue && p.ImportedDate.Value <= importedBefore.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.PackageNumber.ToLower().Contains(term) ||
                p.FileName.ToLower().Contains(term) ||
                (p.ProcessingNotes != null && p.ProcessingNotes.ToLower().Contains(term)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "packagenumber" => sortDescending
                ? query.OrderByDescending(p => p.PackageNumber)
                : query.OrderBy(p => p.PackageNumber),
            "filename" => sortDescending
                ? query.OrderByDescending(p => p.FileName)
                : query.OrderBy(p => p.FileName),
            "status" => sortDescending
                ? query.OrderByDescending(p => p.Status)
                : query.OrderBy(p => p.Status),
            "importeddate" => sortDescending
                ? query.OrderByDescending(p => p.ImportedDate)
                : query.OrderBy(p => p.ImportedDate),
            "exporteddate" => sortDescending
                ? query.OrderByDescending(p => p.PackageExportedDate)
                : query.OrderBy(p => p.PackageExportedDate),
            _ => sortDescending
                ? query.OrderByDescending(p => p.CreatedAtUtc)
                : query.OrderBy(p => p.CreatedAtUtc)
        };

        // Apply pagination
        var packages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (packages, totalCount);
    }

    // ==================== CONFLICT TRACKING ====================

    public async Task<List<ImportPackage>> GetWithUnresolvedConflictsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .Where(p => p.ConflictCount > 0
                && !p.AreConflictsResolved
                && !p.IsDeleted)
            .OrderByDescending(p => p.ConflictCount)
            .ToListAsync(cancellationToken);
    }

    // ==================== AGGREGATE QUERIES ====================

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .Where(p => !p.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ImportPackages
            .AnyAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    // ==================== QUERYABLE ACCESS ====================

    public IQueryable<ImportPackage> GetQueryable()
    {
        return _context.ImportPackages
            .Where(p => !p.IsDeleted)
            .AsQueryable();
    }
}
