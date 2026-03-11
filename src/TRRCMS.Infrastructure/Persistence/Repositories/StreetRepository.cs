using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Street repository implementation.
/// Uses PostGIS spatial queries for bounding box intersection.
/// </summary>
public class StreetRepository : IStreetRepository
{
    private readonly ApplicationDbContext _context;
    private readonly GeometryFactory _geometryFactory;

    public StreetRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<Street?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Streets
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Street?> GetByIdentifierAsync(int identifier, CancellationToken cancellationToken = default)
    {
        return await _context.Streets
            .FirstOrDefaultAsync(s => s.Identifier == identifier, cancellationToken);
    }

    public async Task<List<Street>> GetInBoundingBoxAsync(
        decimal northEastLat, decimal northEastLng,
        decimal southWestLat, decimal southWestLng,
        CancellationToken cancellationToken = default)
    {
        var envelope = new Envelope(
            (double)southWestLng, (double)northEastLng,
            (double)southWestLat, (double)northEastLat);

        var boundingBox = _geometryFactory.ToGeometry(envelope);
        boundingBox.SRID = 4326;

        return await _context.Streets
            .Where(s => s.Geometry != null && s.Geometry.Intersects(boundingBox))
            .OrderBy(s => s.Identifier)
            .ToListAsync(cancellationToken);
    }

    public async Task<Street> AddAsync(Street street, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Streets.AddAsync(street, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(Street street, CancellationToken cancellationToken = default)
    {
        _context.Streets.Update(street);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Street street, CancellationToken cancellationToken = default)
    {
        _context.Streets.Remove(street);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
