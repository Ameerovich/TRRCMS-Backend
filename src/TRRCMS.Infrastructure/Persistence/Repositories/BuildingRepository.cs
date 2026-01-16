using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class BuildingRepository : IBuildingRepository
{
    private readonly ApplicationDbContext _context;

    public BuildingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Buildings
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<Building?> GetByBuildingIdAsync(string buildingId, CancellationToken cancellationToken = default)
    {
        return await _context.Buildings
            .FirstOrDefaultAsync(b => b.BuildingId == buildingId && !b.IsDeleted, cancellationToken);
    }

    public async Task<List<Building>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Buildings
            .Where(b => !b.IsDeleted)
            .OrderBy(b => b.BuildingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Building> AddAsync(Building building, CancellationToken cancellationToken = default)
    {
        await _context.Buildings.AddAsync(building, cancellationToken);
        return building;
    }

    public async Task UpdateAsync(Building building, CancellationToken cancellationToken = default)
    {
        _context.Buildings.Update(building);
        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public IQueryable<Building> GetQueryable()
    {
        return _context.Buildings
            .Where(b => !b.IsDeleted)
            .AsQueryable();
    }

    public async Task<(List<Building> Buildings, int TotalCount)> SearchBuildingsAsync(
        string? governorateCode = null,
        string? districtCode = null,
        string? subDistrictCode = null,
        string? communityCode = null,
        string? neighborhoodCode = null,
        string? buildingId = null,
        string? buildingNumber = null,
        string? address = null,
        decimal? latitude = null,
        decimal? longitude = null,
        int? radiusMeters = null,
        BuildingStatus? status = null,
        BuildingType? buildingType = null,
        DamageLevel? damageLevel = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        // Start with base query
        var query = _context.Buildings
            .Where(b => !b.IsDeleted)
            .AsQueryable();

        // Apply administrative hierarchy filters
        if (!string.IsNullOrWhiteSpace(governorateCode))
            query = query.Where(b => b.GovernorateCode == governorateCode);

        if (!string.IsNullOrWhiteSpace(districtCode))
            query = query.Where(b => b.DistrictCode == districtCode);

        if (!string.IsNullOrWhiteSpace(subDistrictCode))
            query = query.Where(b => b.SubDistrictCode == subDistrictCode);

        if (!string.IsNullOrWhiteSpace(communityCode))
            query = query.Where(b => b.CommunityCode == communityCode);

        if (!string.IsNullOrWhiteSpace(neighborhoodCode))
            query = query.Where(b => b.NeighborhoodCode == neighborhoodCode);

        // Apply direct identifier filters
        if (!string.IsNullOrWhiteSpace(buildingId))
            query = query.Where(b => b.BuildingId == buildingId);

        if (!string.IsNullOrWhiteSpace(buildingNumber))
            query = query.Where(b => b.BuildingNumber == buildingNumber);

        // Apply text search (partial match on address)
        if (!string.IsNullOrWhiteSpace(address))
            query = query.Where(b => b.Address != null && b.Address.Contains(address));

        // Apply spatial filter (bounding box approximation)
        if (latitude.HasValue && longitude.HasValue && radiusMeters.HasValue)
        {
            // Calculate approximate bounding box
            // 1 degree latitude ≈ 111 km
            // 1 degree longitude ≈ 111 km * cos(latitude)
            var latDelta = (decimal)(radiusMeters.Value / 111000.0);
            var lngDelta = (decimal)(radiusMeters.Value / (111000.0 * Math.Cos((double)latitude.Value * Math.PI / 180)));

            var minLat = latitude.Value - latDelta;
            var maxLat = latitude.Value + latDelta;
            var minLng = longitude.Value - lngDelta;
            var maxLng = longitude.Value + lngDelta;

            query = query.Where(b =>
                b.Latitude != null &&
                b.Longitude != null &&
                b.Latitude >= minLat &&
                b.Latitude <= maxLat &&
                b.Longitude >= minLng &&
                b.Longitude <= maxLng);
        }

        // Apply attribute filters
        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        if (buildingType.HasValue)
            query = query.Where(b => b.BuildingType == buildingType.Value);

        if (damageLevel.HasValue)
            query = query.Where(b => b.DamageLevel == damageLevel.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = (sortBy?.ToLower()) switch
        {
            "buildingid" => sortDescending
                ? query.OrderByDescending(b => b.BuildingId)
                : query.OrderBy(b => b.BuildingId),
            "createddate" => sortDescending
                ? query.OrderByDescending(b => b.CreatedAtUtc)
                : query.OrderBy(b => b.CreatedAtUtc),
            _ => query.OrderBy(b => b.BuildingId) // Default sort
        };

        // Apply pagination
        var buildings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (buildings, totalCount);
    }

}