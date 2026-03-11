using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Street entity operations
/// </summary>
public interface IStreetRepository
{
    Task<Street?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Street?> GetByIdentifierAsync(int identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get streets intersecting a bounding box for map rendering
    /// </summary>
    Task<List<Street>> GetInBoundingBoxAsync(
        decimal northEastLat, decimal northEastLng,
        decimal southWestLat, decimal southWestLng,
        CancellationToken cancellationToken = default);

    Task<Street> AddAsync(Street street, CancellationToken cancellationToken = default);
    Task UpdateAsync(Street street, CancellationToken cancellationToken = default);
    Task DeleteAsync(Street street, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
