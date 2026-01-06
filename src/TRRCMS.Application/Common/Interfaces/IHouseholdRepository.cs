using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Household entity operations
/// </summary>
public interface IHouseholdRepository
{
    /// <summary>
    /// Get household by ID
    /// </summary>
    Task<Household?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all households
    /// </summary>
    Task<IEnumerable<Household>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get households by property unit ID
    /// </summary>
    Task<IEnumerable<Household>> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if household exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new household
    /// </summary>
    Task<Household> AddAsync(Household household, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update household
    /// </summary>
    Task UpdateAsync(Household household, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}