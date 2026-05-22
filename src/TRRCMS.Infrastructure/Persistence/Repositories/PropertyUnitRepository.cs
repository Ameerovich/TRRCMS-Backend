using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

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

    public async Task<PropertyUnit?> GetByBuildingAndIdentifierAsync(
     Guid buildingId,
     string unitIdentifier,
     CancellationToken cancellationToken = default)
    {
        return await _context.PropertyUnits
            .FirstOrDefaultAsync(
                pu => pu.BuildingId == buildingId
                && pu.UnitIdentifier == unitIdentifier
                && !pu.IsDeleted,
                cancellationToken);
    }

    public async Task<PropertyUnit?> GetByBuildingAndIdentifierIncludingDeletedAsync(
        Guid buildingId,
        string unitIdentifier,
        CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters() bypasses the global !IsDeleted filter so the result
        // mirrors exactly what the (unfiltered) unique index will enforce on insert.
        return await _context.PropertyUnits
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                pu => pu.BuildingId == buildingId
                && pu.UnitIdentifier == unitIdentifier,
                cancellationToken);
    }

    public async Task<List<PropertyUnit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PropertyUnits
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.BuildingId)
            .ThenBy(p => p.UnitIdentifier)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<PropertyUnit> Items, int TotalCount)> GetPendingIdentifierReconciliationAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _context.PropertyUnits
            .Include(p => p.Building)
            .Where(p => !p.IsDeleted && p.UnitIdentifierAdjustedByKeepSeparate)
            .OrderByDescending(p => p.LastModifiedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<List<PropertyUnit>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyUnits
            .Where(p => !p.IsDeleted && p.BuildingId == buildingId)
            .OrderBy(p => p.FloorNumber)
            .ThenBy(p => p.UnitIdentifier)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PropertyUnit>> GetFilteredAsync(
        Guid? buildingId,
        PropertyUnitType? unitType,
        PropertyUnitStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PropertyUnits
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        // Apply filters (AND-combined)
        if (buildingId.HasValue)
            query = query.Where(p => p.BuildingId == buildingId.Value);

        if (unitType.HasValue)
            query = query.Where(p => p.UnitType == unitType.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        // Order by BuildingId, then FloorNumber, then UnitIdentifier
        return await query
            .OrderBy(p => p.BuildingId)
            .ThenBy(p => p.FloorNumber)
            .ThenBy(p => p.UnitIdentifier)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PropertyUnit propertyUnit, CancellationToken cancellationToken = default)
    {
        await _context.PropertyUnits.AddAsync(propertyUnit, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyUnits
            .Where(p => !p.IsDeleted)
            .AnyAsync(p => p.Id == id, cancellationToken);
    }
    /// <summary>
    /// Update existing property unit
    /// </summary>
    public Task UpdateAsync(PropertyUnit propertyUnit, CancellationToken cancellationToken = default)
    {
        _context.PropertyUnits.Update(propertyUnit);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<PropertyUnit?> GetByBuildingCodeAndUnitIdentifierAsync(
        string buildingCode,
        string unitIdentifier,
        CancellationToken cancellationToken = default)
    {
        return await _context.PropertyUnits
            .Include(pu => pu.Building)
            .Where(pu => !pu.IsDeleted
                && pu.Building.BuildingId == buildingCode
                && pu.UnitIdentifier == unitIdentifier)
            .FirstOrDefaultAsync(cancellationToken);
    }
}