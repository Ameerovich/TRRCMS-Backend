using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for SubDistrict entity
/// </summary>
public class SubDistrictRepository : ISubDistrictRepository
{
    private readonly ApplicationDbContext _context;

    public SubDistrictRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubDistrict>> GetAllAsync(
        string? governorateCode = null,
        string? districtCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.SubDistricts
            .Where(sd => !sd.IsDeleted && sd.IsActive);

        if (!string.IsNullOrWhiteSpace(governorateCode))
        {
            query = query.Where(sd => sd.GovernorateCode == governorateCode);
        }

        if (!string.IsNullOrWhiteSpace(districtCode))
        {
            query = query.Where(sd => sd.DistrictCode == districtCode);
        }

        return await query
            .OrderBy(sd => sd.GovernorateCode)
            .ThenBy(sd => sd.DistrictCode)
            .ThenBy(sd => sd.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubDistrict?> GetByCodeAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.SubDistricts
            .Where(sd => !sd.IsDeleted &&
                        sd.GovernorateCode == governorateCode &&
                        sd.DistrictCode == districtCode &&
                        sd.Code == subDistrictCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(SubDistrict subDistrict, CancellationToken cancellationToken = default)
    {
        await _context.SubDistricts.AddAsync(subDistrict, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.SubDistricts
            .Where(sd => !sd.IsDeleted)
            .AnyAsync(sd => sd.GovernorateCode == governorateCode &&
                           sd.DistrictCode == districtCode &&
                           sd.Code == subDistrictCode,
                     cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
