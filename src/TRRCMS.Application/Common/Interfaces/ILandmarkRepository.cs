using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Landmark entity operations
/// </summary>
public interface ILandmarkRepository
{
    Task<Landmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Landmark?> GetByIdentifierAsync(int identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get landmarks within a bounding box for map rendering
    /// </summary>
    Task<List<Landmark>> GetInBoundingBoxAsync(
        decimal northEastLat, decimal northEastLng,
        decimal southWestLat, decimal southWestLng,
        LandmarkType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search landmarks by name (partial match) with optional type filter.
    /// Used by desk officers to locate landmarks based on applicant descriptions.
    /// </summary>
    Task<List<Landmark>> SearchByNameAsync(
        string query,
        LandmarkType? type = null,
        int maxResults = 50,
        CancellationToken cancellationToken = default);

    Task<Landmark> AddAsync(Landmark landmark, CancellationToken cancellationToken = default);
    Task UpdateAsync(Landmark landmark, CancellationToken cancellationToken = default);
    Task DeleteAsync(Landmark landmark, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
