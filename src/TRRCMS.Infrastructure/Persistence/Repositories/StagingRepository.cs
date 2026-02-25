using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation for all staging entities.
/// Uses EF Core DbSet&lt;T&gt; resolved from ApplicationDbContext.
/// 
/// All 8 staging entity types share the same data access patterns via
/// <see cref="BaseStagingEntity"/> — this single implementation covers them all.
/// 
/// Registered as open generic: IStagingRepository&lt;T&gt; → StagingRepository&lt;T&gt;
/// </summary>
/// <typeparam name="T">A concrete staging entity inheriting from <see cref="BaseStagingEntity"/>.</typeparam>
public class StagingRepository<T> : IStagingRepository<T> where T : BaseStagingEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public StagingRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    // ==================== BASIC CRUD ====================

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // ==================== QUERY BY PACKAGE ====================

    public async Task<List<T>> GetByPackageIdAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.ImportPackageId == importPackageId)
            .OrderBy(e => e.StagedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByPackageAndOriginalIdAsync(
        Guid importPackageId,
        Guid originalEntityId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                e => e.ImportPackageId == importPackageId
                  && e.OriginalEntityId == originalEntityId,
                cancellationToken);
    }

    public async Task<int> GetCountByPackageIdAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.ImportPackageId == importPackageId)
            .CountAsync(cancellationToken);
    }

    // ==================== QUERY BY VALIDATION STATUS ====================

    public async Task<List<T>> GetByPackageAndStatusAsync(
        Guid importPackageId,
        StagingValidationStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.ImportPackageId == importPackageId
                     && e.ValidationStatus == status)
            .OrderBy(e => e.StagedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<StagingValidationStatus, int>> GetStatusCountsByPackageAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.ImportPackageId == importPackageId)
            .GroupBy(e => e.ValidationStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    // ==================== COMMIT QUERIES ====================

    public async Task<List<T>> GetApprovedForCommitAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.ImportPackageId == importPackageId
                     && e.IsApprovedForCommit
                     && e.CommittedEntityId == null) // Skip already-committed (safe re-commit)
            .OrderBy(e => e.StagedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<T>> GetCommittedAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.ImportPackageId == importPackageId
                     && e.CommittedEntityId != null)
            .OrderBy(e => e.StagedAtUtc)
            .ToListAsync(cancellationToken);
    }

    // ==================== BULK OPERATIONS ====================

    public async Task<int> DeleteByPackageIdAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        // EF Core 7+ ExecuteDeleteAsync for efficient bulk delete without loading entities
        return await _dbSet
            .Where(e => e.ImportPackageId == importPackageId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    // ==================== QUERYABLE ACCESS ====================

    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }
}
