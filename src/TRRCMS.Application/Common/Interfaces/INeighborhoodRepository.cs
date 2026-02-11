using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Neighborhood reference data repository.
/// Provides spatial queries for map navigation and boundary rendering.
/// </summary>
public interface INeighborhoodRepository
{
    /// <summary>
    /// Get all active neighborhoods (optionally filtered by parent hierarchy)
    /// </summary>
    Task<List<Neighborhood>> GetAllAsync(
        string? governorateCode = null,
        string? districtCode = null,
        string? subDistrictCode = null,
        string? communityCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get neighborhood by its code within the hierarchy
    /// </summary>
    Task<Neighborhood?> GetByCodeAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        string neighborhoodCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get neighborhood by full composite code (12 digits)
    /// </summary>
    Task<Neighborhood?> GetByFullCodeAsync(
        string fullCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get neighborhoods whose boundaries intersect a bounding box (for map viewport)
    /// </summary>
    Task<List<Neighborhood>> GetInBoundingBoxAsync(
        decimal southWestLat,
        decimal southWestLng,
        decimal northEastLat,
        decimal northEastLng,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the neighborhood that contains a given point (for building validation)
    /// </summary>
    Task<Neighborhood?> GetContainingPointAsync(
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default);

    Task<Neighborhood> AddAsync(Neighborhood neighborhood, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
