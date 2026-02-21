using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Community entity
/// </summary>
public interface ICommunityRepository
{
    /// <summary>
    /// Get all communities, optionally filtered by governorate, district, and sub-district
    /// </summary>
    Task<List<Community>> GetAllAsync(
        string? governorateCode = null,
        string? districtCode = null,
        string? subDistrictCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get community by codes
    /// </summary>
    Task<Community?> GetByCodeAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new community
    /// </summary>
    Task AddAsync(Community community, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if community exists by codes
    /// </summary>
    Task<bool> ExistsAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
