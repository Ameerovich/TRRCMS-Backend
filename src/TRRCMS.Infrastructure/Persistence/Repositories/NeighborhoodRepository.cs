using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Neighborhood repository implementation.
/// Uses PostGIS spatial queries for bounding box and point containment.
/// </summary>
public class NeighborhoodRepository : INeighborhoodRepository
{
    private readonly ApplicationDbContext _context;
    private readonly GeometryFactory _geometryFactory;

    public NeighborhoodRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<List<Neighborhood>> GetAllAsync(
        string? governorateCode = null,
        string? districtCode = null,
        string? subDistrictCode = null,
        string? communityCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Neighborhoods
            .Where(n => !n.IsDeleted && n.IsActive);

        if (!string.IsNullOrWhiteSpace(governorateCode))
            query = query.Where(n => n.GovernorateCode == governorateCode);

        if (!string.IsNullOrWhiteSpace(districtCode))
            query = query.Where(n => n.DistrictCode == districtCode);

        if (!string.IsNullOrWhiteSpace(subDistrictCode))
            query = query.Where(n => n.SubDistrictCode == subDistrictCode);

        if (!string.IsNullOrWhiteSpace(communityCode))
            query = query.Where(n => n.CommunityCode == communityCode);

        return await query
            .OrderBy(n => n.NeighborhoodCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<Neighborhood?> GetByCodeAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        string neighborhoodCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.Neighborhoods
            .Where(n => !n.IsDeleted &&
                        n.GovernorateCode == governorateCode &&
                        n.DistrictCode == districtCode &&
                        n.SubDistrictCode == subDistrictCode &&
                        n.CommunityCode == communityCode &&
                        n.NeighborhoodCode == neighborhoodCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Neighborhood?> GetByFullCodeAsync(
        string fullCode,
        CancellationToken cancellationToken = default)
    {
        return await _context.Neighborhoods
            .Where(n => !n.IsDeleted && n.FullCode == fullCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Neighborhood>> GetInBoundingBoxAsync(
        decimal southWestLat,
        decimal southWestLng,
        decimal northEastLat,
        decimal northEastLng,
        CancellationToken cancellationToken = default)
    {
        var envelope = new Envelope(
            (double)southWestLng, (double)northEastLng,
            (double)southWestLat, (double)northEastLat);

        var boundingBox = _geometryFactory.ToGeometry(envelope);
        boundingBox.SRID = 4326;

        return await _context.Neighborhoods
            .Where(n => !n.IsDeleted && n.IsActive &&
                        n.BoundaryGeometry != null &&
                        n.BoundaryGeometry.Intersects(boundingBox))
            .OrderBy(n => n.NeighborhoodCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<Neighborhood?> GetContainingPointAsync(
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default)
    {
        var point = _geometryFactory.CreatePoint(
            new Coordinate((double)longitude, (double)latitude));
        point.SRID = 4326;

        return await _context.Neighborhoods
            .Where(n => !n.IsDeleted && n.IsActive &&
                        n.BoundaryGeometry != null &&
                        n.BoundaryGeometry.Contains(point))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Neighborhood> AddAsync(
        Neighborhood neighborhood,
        CancellationToken cancellationToken = default)
    {
        var entry = await _context.Neighborhoods.AddAsync(neighborhood, cancellationToken);
        return entry.Entity;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
