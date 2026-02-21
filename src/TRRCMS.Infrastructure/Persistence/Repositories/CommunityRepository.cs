using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Community entity
/// </summary>
public class CommunityRepository : ICommunityRepository
{
    private readonly ApplicationDbContext _context;

    public CommunityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Community>> GetAllAsync(
        string? governorateCode = null,
        string? districtCode = null,
        string? subDistrictCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Communities
            .Where(c => !c.IsDeleted && c.IsActive);

        if (!string.IsNullOrWhiteSpace(governorateCode))
        {
            query = query.Where(c => c.GovernorateCode == governorateCode);
        }

        if (!string.IsNullOrWhiteSpace(districtCode))
        {
            query = query.Where(c => c.DistrictCode == districtCode);
        }

        if (!string.IsNullOrWhiteSpace(subDistrictCode))
        {
            query = query.Where(c => c.SubDistrictCode == subDistrictCode);
        }

        return await query
            .OrderBy(c => c.GovernorateCode)
            .ThenBy(c => c.DistrictCode)
            .ThenBy(c => c.SubDistrictCode)
            .ThenBy(c => c.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<Community?> GetByCodeAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.Communities
            .Where(c => !c.IsDeleted &&
                       c.GovernorateCode == governorateCode &&
                       c.DistrictCode == districtCode &&
                       c.SubDistrictCode == subDistrictCode &&
                       c.Code == communityCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Community community, CancellationToken cancellationToken = default)
    {
        await _context.Communities.AddAsync(community, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.Communities
            .Where(c => !c.IsDeleted)
            .AnyAsync(c => c.GovernorateCode == governorateCode &&
                          c.DistrictCode == districtCode &&
                          c.SubDistrictCode == subDistrictCode &&
                          c.Code == communityCode,
                     cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
