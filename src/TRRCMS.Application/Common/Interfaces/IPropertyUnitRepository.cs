using TRRCMS.Domain.Entities;

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
    /// Get property unit by building and unit identifier
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
    /// Add new property unit
    /// </summary>
    Task AddAsync(PropertyUnit propertyUnit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing property unit
    /// </summary>
    void Update(PropertyUnit propertyUnit);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}