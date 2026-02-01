using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Building repository implementation with PostGIS spatial query support
/// </summary>
public class BuildingRepository : IBuildingRepository
{
    private readonly ApplicationDbContext _context;
    private readonly GeometryFactory _geometryFactory;
    private readonly WKTReader _wktReader;

    public BuildingRepository(ApplicationDbContext context)
    {
        _context = context;
        // SRID 4326 = WGS84 (GPS coordinate system)
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        _wktReader = new WKTReader(_geometryFactory);
    }

    // ==================== CRUD OPERATIONS ====================

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

    // ==================== SEARCH WITH FILTERS ====================

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

        // ============================================================
        // UPDATED: Apply buildingId filter with PARTIAL MATCH support
        // Supports both formatted (01-01-01-001-001-00001) and 
        // unformatted (01010100100100001) input
        // ============================================================
        if (!string.IsNullOrWhiteSpace(buildingId))
        {
            // Remove dashes if user entered formatted version
            // Example: "01-01-01" becomes "010101"
            var normalizedBuildingId = buildingId.Replace("-", "");
            query = query.Where(b => b.BuildingId.Contains(normalizedBuildingId));
        }

        // Apply exact match on building number
        if (!string.IsNullOrWhiteSpace(buildingNumber))
            query = query.Where(b => b.BuildingNumber == buildingNumber);

        // Apply text search on address
        if (!string.IsNullOrWhiteSpace(address))
            query = query.Where(b => b.Address != null && b.Address.Contains(address));

        // Apply spatial filter using PostGIS
        if (latitude.HasValue && longitude.HasValue && radiusMeters.HasValue)
        {
            var point = _geometryFactory.CreatePoint(
                new Coordinate((double)longitude.Value, (double)latitude.Value));

            query = query.Where(b =>
                b.BuildingGeometry != null &&
                b.BuildingGeometry.IsWithinDistance(point, radiusMeters.Value));
        }

        // Apply attribute filters
        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        if (buildingType.HasValue)
            query = query.Where(b => b.BuildingType == buildingType.Value);

        if (damageLevel.HasValue)
            query = query.Where(b => b.DamageLevel == damageLevel.Value);

        // Get total count
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
            "status" => sortDescending
                ? query.OrderByDescending(b => b.Status)
                : query.OrderBy(b => b.Status),
            "buildingtype" => sortDescending
                ? query.OrderByDescending(b => b.BuildingType)
                : query.OrderBy(b => b.BuildingType),
            _ => query.OrderBy(b => b.BuildingId)
        };

        // Apply pagination
        var buildings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (buildings, totalCount);
    }

    // ==================== SPATIAL QUERIES (PostGIS) ====================

    public async Task<List<Building>> GetBuildingsWithinRadiusAsync(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        CancellationToken cancellationToken = default)
    {
        var point = _geometryFactory.CreatePoint(
            new Coordinate((double)longitude, (double)latitude));

        return await _context.Buildings
            .Where(b => !b.IsDeleted &&
                        b.BuildingGeometry != null &&
                        b.BuildingGeometry.IsWithinDistance(point, radiusMeters))
            .OrderBy(b => b.BuildingGeometry!.Distance(point))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Building>> GetBuildingsInPolygonAsync(
        string polygonWkt,
        CancellationToken cancellationToken = default)
    {
        var polygon = _wktReader.Read(polygonWkt);
        polygon.SRID = 4326;

        return await _context.Buildings
            .Where(b => !b.IsDeleted &&
                        b.BuildingGeometry != null &&
                        b.BuildingGeometry.Within(polygon))
            .OrderBy(b => b.BuildingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Building>> GetBuildingsInBoundingBoxAsync(
        decimal minLatitude,
        decimal maxLatitude,
        decimal minLongitude,
        decimal maxLongitude,
        BuildingType? buildingType = null,
        BuildingStatus? status = null,
        int maxResults = 10000,
        CancellationToken cancellationToken = default)
    {
        // Create bounding box
        var envelope = new Envelope(
            (double)minLongitude, (double)maxLongitude,
            (double)minLatitude, (double)maxLatitude);

        var boundingBox = _geometryFactory.ToGeometry(envelope);
        boundingBox.SRID = 4326;

        var query = _context.Buildings
            .Where(b => !b.IsDeleted &&
                        b.BuildingGeometry != null &&
                        b.BuildingGeometry.Intersects(boundingBox));

        if (buildingType.HasValue)
            query = query.Where(b => b.BuildingType == buildingType.Value);

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        return await query
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<double?> GetDistanceBetweenBuildingsAsync(
        Guid buildingId1,
        Guid buildingId2,
        CancellationToken cancellationToken = default)
    {
        var building1 = await _context.Buildings
            .Where(b => b.Id == buildingId1 && !b.IsDeleted)
            .Select(b => b.BuildingGeometry)
            .FirstOrDefaultAsync(cancellationToken);

        var building2 = await _context.Buildings
            .Where(b => b.Id == buildingId2 && !b.IsDeleted)
            .Select(b => b.BuildingGeometry)
            .FirstOrDefaultAsync(cancellationToken);

        if (building1 == null || building2 == null)
            return null;

        // Distance in degrees, convert to meters
        var distanceDegrees = building1.Distance(building2);
        var avgLatitude = (building1.Centroid.Y + building2.Centroid.Y) / 2;
        var metersPerDegree = 111139 * Math.Cos(avgLatitude * Math.PI / 180);

        return distanceDegrees * metersPerDegree;
    }

    public async Task<List<Building>> GetBuildingsIntersectingAsync(
        string geometryWkt,
        Guid? excludeBuildingId = null,
        CancellationToken cancellationToken = default)
    {
        var geometry = _wktReader.Read(geometryWkt);
        geometry.SRID = 4326;

        var query = _context.Buildings
            .Where(b => !b.IsDeleted &&
                        b.BuildingGeometry != null &&
                        b.BuildingGeometry.Intersects(geometry));

        if (excludeBuildingId.HasValue)
            query = query.Where(b => b.Id != excludeBuildingId.Value);

        return await query
            .OrderBy(b => b.BuildingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Building>> GetNearestBuildingsAsync(
        decimal latitude,
        decimal longitude,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var point = _geometryFactory.CreatePoint(
            new Coordinate((double)longitude, (double)latitude));

        return await _context.Buildings
            .Where(b => !b.IsDeleted && b.BuildingGeometry != null)
            .OrderBy(b => b.BuildingGeometry!.Distance(point))
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
