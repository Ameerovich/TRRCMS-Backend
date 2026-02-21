using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for District entity
/// </summary>
public interface IDistrictRepository
{
    /// <summary>
    /// Get all districts, optionally filtered by governorate
    /// </summary>
    Task<List<District>> GetAllAsync(string? governorateCode = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get district by codes
    /// </summary>
    Task<District?> GetByCodeAsync(string governorateCode, string districtCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new district
    /// </summary>
    Task AddAsync(District district, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if district exists by codes
    /// </summary>
    Task<bool> ExistsAsync(string governorateCode, string districtCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
