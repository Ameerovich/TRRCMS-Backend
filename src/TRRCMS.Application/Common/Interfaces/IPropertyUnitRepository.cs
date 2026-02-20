using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for PropertyUnit entity
/// </summary>
public interface IPropertyUnitRepository
{
    /// <summary>
    /// Get property unit by ID
    /// </summary>
    Task<PropertyUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get property unit by building and unit identifier (for duplicate check)
    /// </summary>
    Task<PropertyUnit?> GetByBuildingAndIdentifierAsync(Guid buildingId, string unitIdentifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all property units
    /// </summary>
    Task<List<PropertyUnit>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all property units for a specific building
    /// </summary>
    Task<List<PropertyUnit>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get property units with optional filtering
    /// Filters are AND-combined (all must match if provided)
    /// Results are ordered by BuildingId, FloorNumber, then UnitIdentifier
    /// </summary>
    /// <param name="buildingId">Filter by building ID (optional)</param>
    /// <param name="unitType">Filter by property unit type (optional)</param>
    /// <param name="status">Filter by property unit status (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered list of property units</returns>
    Task<List<PropertyUnit>> GetFilteredAsync(
        Guid? buildingId,
        PropertyUnitType? unitType,
        PropertyUnitStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new property unit
    /// </summary>
    Task AddAsync(PropertyUnit propertyUnit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing property unit
    /// </summary>
    Task UpdateAsync(PropertyUnit propertyUnit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}