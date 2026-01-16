using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

public interface IBuildingRepository
{
    Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Building?> GetByBuildingIdAsync(string buildingId, CancellationToken cancellationToken = default);
    Task<List<Building>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Building> AddAsync(Building building, CancellationToken cancellationToken = default);
    Task UpdateAsync(Building building, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search buildings with multiple filters and pagination
    /// Returns tuple of (buildings, totalCount)
    /// </summary>
    Task<(List<Building> Buildings, int TotalCount)> SearchBuildingsAsync(
        string? governorateCode = null,
        string? districtCode = null,
        string? subDistrictCode = null,
        string? communityCode = null,
        string? neighborhoodCode = null,
        string? buildingId = null,
        string? buildingNumber = null,
        string? address = null,
        decimal? latitude = null,
        decimal? longitude = null,
        int? radiusMeters = null,
        BuildingStatus? status = null,
        BuildingType? buildingType = null,
        DamageLevel? damageLevel = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// Get queryable for custom queries
    /// Used by GetBuildingsForMap for efficient projection
    /// </summary>
    IQueryable<Building> GetQueryable();
}