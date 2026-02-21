using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for SubDistrict entity
/// </summary>
public interface ISubDistrictRepository
{
    /// <summary>
    /// Get all sub-districts, optionally filtered by governorate and district
    /// </summary>
    Task<List<SubDistrict>> GetAllAsync(
        string? governorateCode = null,
        string? districtCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get sub-district by codes
    /// </summary>
    Task<SubDistrict?> GetByCodeAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new sub-district
    /// </summary>
    Task AddAsync(SubDistrict subDistrict, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if sub-district exists by codes
    /// </summary>
    Task<bool> ExistsAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
