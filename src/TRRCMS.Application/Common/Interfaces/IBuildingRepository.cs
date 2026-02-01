using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Building repository interface
/// Supports CRUD operations and PostGIS spatial queries
/// </summary>
public interface IBuildingRepository
{
    // ==================== CRUD OPERATIONS ====================

    Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Building?> GetByBuildingIdAsync(string buildingId, CancellationToken cancellationToken = default);

    Task<List<Building>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Building> AddAsync(Building building, CancellationToken cancellationToken = default);

    Task UpdateAsync(Building building, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queryable for custom queries
    /// </summary>
    IQueryable<Building> GetQueryable();

    // ==================== REFERENTIAL INTEGRITY CHECKS (for Delete) ====================

    /// <summary>
    /// Check if building has any non-deleted property units
    /// </summary>
    Task<bool> HasPropertyUnitsAsync(Guid buildingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if building has any active surveys (Draft or Completed but not Finalized)
    /// </summary>
    Task<bool> HasActiveSurveysAsync(Guid buildingId, CancellationToken cancellationToken = default);

    // ==================== SEARCH WITH FILTERS ====================

    /// <summary>
    /// Search buildings with multiple filters and pagination
    /// Supports PostGIS spatial filtering when coordinates and radius provided
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

    // ==================== SPATIAL QUERIES (PostGIS) ====================

    /// <summary>
    /// Find buildings within a specified radius from a point
    /// Uses PostGIS ST_DWithin for accurate distance calculation
    /// </summary>
    Task<List<Building>> GetBuildingsWithinRadiusAsync(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find buildings within a polygon area (simple version)
    /// Uses PostGIS ST_Within
    /// </summary>
    Task<List<Building>> GetBuildingsInPolygonAsync(
        string polygonWkt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find buildings within a polygon area with filters and pagination
    /// Uses PostGIS ST_Within
    /// </summary>
    Task<(List<Building> Buildings, int TotalCount)> SearchBuildingsInPolygonAsync(
        string polygonWkt,
        BuildingType? buildingType = null,
        BuildingStatus? status = null,
        DamageLevel? damageLevel = null,
        int page = 1,
        int pageSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find buildings within a bounding box (optimized for map display)
    /// Uses PostGIS spatial index
    /// </summary>
    Task<List<Building>> GetBuildingsInBoundingBoxAsync(
        decimal minLatitude,
        decimal maxLatitude,
        decimal minLongitude,
        decimal maxLongitude,
        BuildingType? buildingType = null,
        BuildingStatus? status = null,
        int maxResults = 10000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate distance between two buildings in meters
    /// Uses PostGIS ST_Distance
    /// </summary>
    Task<double?> GetDistanceBetweenBuildingsAsync(
        Guid buildingId1,
        Guid buildingId2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find buildings that intersect with a given geometry
    /// Uses PostGIS ST_Intersects
    /// </summary>
    Task<List<Building>> GetBuildingsIntersectingAsync(
        string geometryWkt,
        Guid? excludeBuildingId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get nearest buildings to a point
    /// </summary>
    Task<List<Building>> GetNearestBuildingsAsync(
        decimal latitude,
        decimal longitude,
        int count = 10,
        CancellationToken cancellationToken = default);
}
