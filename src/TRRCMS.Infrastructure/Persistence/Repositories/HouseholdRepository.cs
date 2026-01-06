using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Household entity
/// </summary>
public class HouseholdRepository : IHouseholdRepository
{
    private readonly ApplicationDbContext _context;

    public HouseholdRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Household?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Households
            .Include(h => h.PropertyUnit)
            .Include(h => h.HeadOfHouseholdPerson)
            .Include(h => h.Members)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Household>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Households
            .Include(h => h.PropertyUnit)
            .Include(h => h.HeadOfHouseholdPerson)
            .Where(h => !h.IsDeleted)
            .OrderBy(h => h.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Household>> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Households
            .Include(h => h.PropertyUnit)
            .Include(h => h.HeadOfHouseholdPerson)
            .Include(h => h.Members)
            .Where(h => h.PropertyUnitId == propertyUnitId && !h.IsDeleted)
            .OrderBy(h => h.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Households
            .AnyAsync(h => h.Id == id && !h.IsDeleted, cancellationToken);
    }

    public async Task<Household> AddAsync(Household household, CancellationToken cancellationToken = default)
    {
        await _context.Households.AddAsync(household, cancellationToken);
        return household;
    }

    public Task UpdateAsync(Household household, CancellationToken cancellationToken = default)
    {
        _context.Households.Update(household);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
