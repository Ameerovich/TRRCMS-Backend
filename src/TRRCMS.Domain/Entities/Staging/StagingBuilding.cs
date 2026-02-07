using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for Building records from .uhc packages.
/// Mirrors the <see cref="Building"/> production entity in an isolated staging area.
/// Records are validated before commit to production (FSD FR-D-4).
/// 
/// Key differences from production Building:
/// - Uses WKT string (<see cref="BuildingGeometryWkt"/>) instead of PostGIS Geometry
///   (converted to native geometry on commit).
/// - No navigation properties (staging records are self-contained).
/// - Inherits <see cref="BaseStagingEntity"/> instead of <see cref="BaseAuditableEntity"/>.
/// 
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingBuilding : BaseStagingEntity
{
    // ==================== BUILDING IDENTIFICATION ====================

    /// <summary>
    /// Composite building identifier (رمز البناء).
    /// Format: GGDDSSCCNCNNBBBBB (17 digits).
    /// Optional in staging — computed from 6 admin codes during commit.
    /// Used for duplicate detection against production Buildings when present.
    /// </summary>
    public string? BuildingId { get; private set; }

    // ==================== ADMINISTRATIVE CODES ====================

    /// <summary>Governorate code (محافظة) — 2 digits.</summary>
    public string GovernorateCode { get; private set; }

    /// <summary>District code (مدينة) — 2 digits.</summary>
    public string DistrictCode { get; private set; }

    /// <summary>Sub-district code (بلدة) — 2 digits.</summary>
    public string SubDistrictCode { get; private set; }

    /// <summary>Community code (قرية) — 3 digits.</summary>
    public string CommunityCode { get; private set; }

    /// <summary>Neighborhood code (حي) — 3 digits.</summary>
    public string NeighborhoodCode { get; private set; }

    /// <summary>Building number within neighborhood — 5 digits.</summary>
    public string BuildingNumber { get; private set; }

    // ==================== LOCATION NAMES ====================

    /// <summary>Governorate name in Arabic (from lookup — optional in staging).</summary>
    public string? GovernorateName { get; private set; }

    /// <summary>District name in Arabic (from lookup — optional in staging).</summary>
    public string? DistrictName { get; private set; }

    /// <summary>Sub-district name in Arabic (from lookup — optional in staging).</summary>
    public string? SubDistrictName { get; private set; }

    /// <summary>Community name in Arabic (from lookup — optional in staging).</summary>
    public string? CommunityName { get; private set; }

    /// <summary>Neighborhood name in Arabic (from lookup — optional in staging).</summary>
    public string? NeighborhoodName { get; private set; }

    // ==================== BUILDING ATTRIBUTES ====================

    /// <summary>Building type classification (نوع البناء).</summary>
    public BuildingType BuildingType { get; private set; }

    /// <summary>Building status — physical condition (حالة البناء).</summary>
    public BuildingStatus Status { get; private set; }

    /// <summary>Damage level assessment (مستوى الضرر).</summary>
    public DamageLevel? DamageLevel { get; private set; }

    // ==================== UNIT COUNTS (from command — required) ====================

    /// <summary>Total number of property units (عدد الوحدات).</summary>
    public int NumberOfPropertyUnits { get; private set; }

    /// <summary>Number of residential apartments (عدد المقاسم).</summary>
    public int NumberOfApartments { get; private set; }

    /// <summary>Number of commercial shops (عدد المحلات).</summary>
    public int NumberOfShops { get; private set; }

    // ==================== FUTURE EXPANSION (optional) ====================

    /// <summary>Number of floors in the building — optional future expansion.</summary>
    public int? NumberOfFloors { get; private set; }

    /// <summary>Year when building was constructed — optional future expansion.</summary>
    public int? YearOfConstruction { get; private set; }

    // ==================== SPATIAL DATA ====================

    /// <summary>
    /// WKT representation of building geometry.
    /// Stored as string in staging; converted to PostGIS Geometry on commit.
    /// </summary>
    public string? BuildingGeometryWkt { get; private set; }

    /// <summary>GPS latitude coordinate (center point).</summary>
    public decimal? Latitude { get; private set; }

    /// <summary>GPS longitude coordinate (center point).</summary>
    public decimal? Longitude { get; private set; }

    // ==================== OPTIONAL FIELDS ====================

    /// <summary>Building address or description.</summary>
    public string? Address { get; private set; }

    /// <summary>Landmark or notable features near the building.</summary>
    public string? Landmark { get; private set; }

    /// <summary>Location description (وصف الموقع).</summary>
    public string? LocationDescription { get; private set; }

    /// <summary>Additional notes (الوصف العام).</summary>
    public string? Notes { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>EF Core constructor.</summary>
    private StagingBuilding() : base()
    {
        GovernorateCode = string.Empty;
        DistrictCode = string.Empty;
        SubDistrictCode = string.Empty;
        CommunityCode = string.Empty;
        NeighborhoodCode = string.Empty;
        BuildingNumber = string.Empty;
    }

    // ==================== FACTORY METHOD ====================

    /// <summary>
    /// Create a new StagingBuilding record from .uhc package data.
    /// Required parameters match CreateBuildingCommand fields.
    /// </summary>
    public static StagingBuilding Create(
        Guid importPackageId,
        Guid originalEntityId,
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        string neighborhoodCode,
        string buildingNumber,
        BuildingType buildingType,
        BuildingStatus status,
        int numberOfPropertyUnits,
        int numberOfApartments,
        int numberOfShops,
        // --- optional: from command ---
        decimal? latitude = null,
        decimal? longitude = null,
        string? buildingGeometryWkt = null,
        string? locationDescription = null,
        string? notes = null,
        // --- optional: auto-generated / lookup / future expansion ---
        string? buildingId = null,
        string? governorateName = null,
        string? districtName = null,
        string? subDistrictName = null,
        string? communityName = null,
        string? neighborhoodName = null,
        DamageLevel? damageLevel = null,
        int? numberOfFloors = null,
        int? yearOfConstruction = null,
        string? address = null,
        string? landmark = null)
    {
        var entity = new StagingBuilding
        {
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            SubDistrictCode = subDistrictCode,
            CommunityCode = communityCode,
            NeighborhoodCode = neighborhoodCode,
            BuildingNumber = buildingNumber,
            BuildingType = buildingType,
            Status = status,
            NumberOfPropertyUnits = numberOfPropertyUnits,
            NumberOfApartments = numberOfApartments,
            NumberOfShops = numberOfShops,
            Latitude = latitude,
            Longitude = longitude,
            BuildingGeometryWkt = buildingGeometryWkt,
            LocationDescription = locationDescription,
            Notes = notes,
            BuildingId = buildingId,
            GovernorateName = governorateName,
            DistrictName = districtName,
            SubDistrictName = subDistrictName,
            CommunityName = communityName,
            NeighborhoodName = neighborhoodName,
            DamageLevel = damageLevel,
            NumberOfFloors = numberOfFloors,
            YearOfConstruction = yearOfConstruction,
            Address = address,
            Landmark = landmark
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
