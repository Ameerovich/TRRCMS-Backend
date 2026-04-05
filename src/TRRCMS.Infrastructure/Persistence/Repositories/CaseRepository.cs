using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class CaseRepository : ICaseRepository
{
    private readonly ApplicationDbContext _context;

    public CaseRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Case?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Cases
            .Include(c => c.Surveys)
            .Include(c => c.Claims)
            .Include(c => c.PersonPropertyRelations)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<Case?> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Cases
            .Include(c => c.Surveys)
            .Include(c => c.Claims)
            .Include(c => c.PersonPropertyRelations)
            .FirstOrDefaultAsync(c => c.PropertyUnitId == propertyUnitId && !c.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsForPropertyUnitAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Cases
            .AnyAsync(c => c.PropertyUnitId == propertyUnitId && !c.IsDeleted, cancellationToken);
    }

    public async Task<(List<Case> Items, int TotalCount)> GetAllAsync(
        CaseLifecycleStatus? status = null,
        Guid? buildingId = null,
        string? buildingCode = null,
        string? unitIdentifier = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Cases
            .Include(c => c.Surveys)
            .Include(c => c.Claims)
            .Include(c => c.PropertyUnit)
            .Where(c => !c.IsDeleted);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(buildingCode))
        {
            var normalizedCode = buildingCode.Replace("-", "");
            query = query.Include(c => c.PropertyUnit.Building)
                .Where(c => c.PropertyUnit.Building.BuildingId == normalizedCode);
        }
        else if (buildingId.HasValue)
        {
            query = query.Where(c => c.PropertyUnit.BuildingId == buildingId.Value);
        }

        if (!string.IsNullOrWhiteSpace(unitIdentifier))
            query = query.Where(c => c.PropertyUnit.UnitIdentifier == unitIdentifier);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cases.CountAsync(c => !c.IsDeleted, cancellationToken);
    }

    public async Task<Dictionary<CaseLifecycleStatus, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cases
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<List<(int Year, int Month, int Count)>> GetMonthlyCreationCountsAsync(
        DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Cases.Where(c => !c.IsDeleted);

        if (from.HasValue)
            query = query.Where(c => c.CreatedAtUtc >= from.Value);
        if (to.HasValue)
            query = query.Where(c => c.CreatedAtUtc <= to.Value);

        var result = await query
            .GroupBy(c => new { c.CreatedAtUtc.Year, c.CreatedAtUtc.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        return result.Select(r => (r.Year, r.Month, r.Count)).ToList();
    }

    public async Task AddAsync(Case entity, CancellationToken cancellationToken = default)
    {
        await _context.Cases.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(Case entity, CancellationToken cancellationToken = default)
    {
        _context.Cases.Update(entity);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
