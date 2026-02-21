using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Governorate entity
/// </summary>
public class GovernorateRepository : IGovernorateRepository
{
    private readonly ApplicationDbContext _context;

    public GovernorateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Governorate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Governorates
            .Where(g => !g.IsDeleted && g.IsActive)
            .OrderBy(g => g.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<Governorate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Governorates
            .Where(g => !g.IsDeleted && g.Code == code)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Governorate governorate, CancellationToken cancellationToken = default)
    {
        await _context.Governorates.AddAsync(governorate, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Governorates
            .Where(g => !g.IsDeleted)
            .AnyAsync(g => g.Code == code, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
