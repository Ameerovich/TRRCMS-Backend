using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Governorate entity
/// </summary>
public interface IGovernorateRepository
{
    /// <summary>
    /// Get all governorates
    /// </summary>
    Task<List<Governorate>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get governorate by code
    /// </summary>
    Task<Governorate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new governorate
    /// </summary>
    Task AddAsync(Governorate governorate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if governorate exists by code
    /// </summary>
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
