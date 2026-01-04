using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for PropertyUnit entity
/// </summary>
public class PropertyUnitRepository : IPropertyUnitRepository
{
    private readonly ApplicationDbContext _context;

    public PropertyUnitRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyUnits
            .Where(p => !p.IsDeleted && p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PropertyUnit?> GetByBuildingAndIdentifierAsync(Guid buildingId, string unitIdentifier, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyUnits
            .Where(p => !p.IsDeleted && p.BuildingId == buildingId && p.UnitIdentifier == unitIdentifier)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PropertyUnit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PropertyUnits
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.BuildingId)
            .ThenBy(p => p.UnitIdentifier)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PropertyUnit>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyUnits
            .Where(p => !p.IsDeleted && p.BuildingId == buildingId)
            .OrderBy(p => p.FloorNumber)
            .ThenBy(p => p.UnitIdentifier)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PropertyUnit propertyUnit, CancellationToken cancellationToken = default)
    {
        await _context.PropertyUnits.AddAsync(propertyUnit, cancellationToken);
    }

    public void Update(PropertyUnit propertyUnit)
    {
        _context.PropertyUnits.Update(propertyUnit);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}