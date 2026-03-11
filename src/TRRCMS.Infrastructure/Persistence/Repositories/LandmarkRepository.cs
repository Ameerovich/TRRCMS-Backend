using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Landmark repository implementation.
/// Uses PostGIS spatial queries for bounding box and name search.
/// </summary>
public class LandmarkRepository : ILandmarkRepository
{
    private readonly ApplicationDbContext _context;
    private readonly GeometryFactory _geometryFactory;

    public LandmarkRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
    }

    public async Task<Landmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Landmarks
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<Landmark?> GetByIdentifierAsync(int identifier, CancellationToken cancellationToken = default)
    {
        return await _context.Landmarks
            .FirstOrDefaultAsync(l => l.Identifier == identifier, cancellationToken);
    }

    public async Task<List<Landmark>> GetInBoundingBoxAsync(
        decimal northEastLat, decimal northEastLng,
        decimal southWestLat, decimal southWestLng,
        LandmarkType? type = null,
        CancellationToken cancellationToken = default)
    {
        var envelope = new Envelope(
            (double)southWestLng, (double)northEastLng,
            (double)southWestLat, (double)northEastLat);

        var boundingBox = _geometryFactory.ToGeometry(envelope);
        boundingBox.SRID = 4326;

        var query = _context.Landmarks
            .Where(l => l.Location != null && l.Location.Intersects(boundingBox));

        if (type.HasValue)
            query = query.Where(l => l.Type == type.Value);

        return await query
            .OrderBy(l => l.Identifier)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Landmark>> SearchByNameAsync(
        string searchQuery,
        LandmarkType? type = null,
        int maxResults = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Landmarks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
            query = query.Where(l => l.Name.Contains(searchQuery));

        if (type.HasValue)
            query = query.Where(l => l.Type == type.Value);

        return await query
            .OrderBy(l => l.Name)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<Landmark> AddAsync(Landmark landmark, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Landmarks.AddAsync(landmark, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(Landmark landmark, CancellationToken cancellationToken = default)
    {
        _context.Landmarks.Update(landmark);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Landmark landmark, CancellationToken cancellationToken = default)
    {
        _context.Landmarks.Remove(landmark);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
