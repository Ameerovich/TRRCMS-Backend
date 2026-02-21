using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for District entity
/// </summary>
public class DistrictRepository : IDistrictRepository
{
    private readonly ApplicationDbContext _context;

    public DistrictRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<District>> GetAllAsync(string? governorateCode = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Districts
            .Where(d => !d.IsDeleted && d.IsActive);

        if (!string.IsNullOrWhiteSpace(governorateCode))
        {
            query = query.Where(d => d.GovernorateCode == governorateCode);
        }

        return await query
            .OrderBy(d => d.GovernorateCode)
            .ThenBy(d => d.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<District?> GetByCodeAsync(string governorateCode, string districtCode, CancellationToken cancellationToken = default)
    {
        return await _context.Districts
            .Where(d => !d.IsDeleted &&
                       d.GovernorateCode == governorateCode &&
                       d.Code == districtCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(District district, CancellationToken cancellationToken = default)
    {
        await _context.Districts.AddAsync(district, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string governorateCode, string districtCode, CancellationToken cancellationToken = default)
    {
        return await _context.Districts
            .Where(d => !d.IsDeleted)
            .AnyAsync(d => d.GovernorateCode == governorateCode && d.Code == districtCode, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
