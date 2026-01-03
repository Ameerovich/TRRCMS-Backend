using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Building entity - represents physical buildings in the system.
/// </summary>
public class Building : BaseAuditableEntity
{
    // ==================== BUSINESS IDENTIFIER ====================

    /// <summary>
    /// Unique 17-digit building identifier (رقم البناء)
    /// Format: GG-DD-SS-CCC-NNN-BBBBB
    /// This is the BUSINESS identifier visible to users
    /// </summary>
    public string BuildingId { get; private set; }

    // ==================== ADMINISTRATIVE CODES ====================

    /// <summary>
    /// Governorate code (رمز المحافظة) - 2 digits
    /// </summary>
    public string GovernorateCode { get; private set; }

    /// <summary>
    /// District code (رمز المنطقة الإدارية) - 2 digits
    /// </summary>
    public string DistrictCode { get; private set; }

    /// <summary>
    /// Sub-district code (رمز الناحية الإدارية) - 2 digits
    /// </summary>
    public string SubDistrictCode { get; private set; }

    /// <summary>
    /// Community code (رمز التجمع العمراني) - 3 digits
    /// </summary>
    public string CommunityCode { get; private set; }

    /// <summary>
    /// Neighborhood code (رمز الحي) - 3 digits
    /// </summary>
    public string NeighborhoodCode { get; private set; }

    /// <summary>
    /// Building number within neighborhood (رقم البناء) - 5 digits
    /// </summary>
    public string BuildingNumber { get; private set; }

    // ==================== LOCATION NAMES (ARABIC) ====================

    /// <summary>
    /// Governorate name in Arabic
    /// </summary>
    public string GovernorateName { get; private set; }

    /// <summary>
    /// District name in Arabic
    /// </summary>
    public string DistrictName { get; private set; }

    /// <summary>
    /// Sub-district name in Arabic
    /// </summary>
    public string SubDistrictName { get; private set; }

    /// <summary>
    /// Community name in Arabic
    /// </summary>
    public string CommunityName { get; private set; }

    /// <summary>
    /// Neighborhood name in Arabic
    /// </summary>
    public string NeighborhoodName { get; private set; }

    // ==================== BUILDING ATTRIBUTES ====================

    /// <summary>
    /// Building type classification (نوع البناء)
    /// </summary>
    public BuildingType BuildingType { get; private set; }

    /// <summary>
    /// Building status - physical condition (حالة البناء)
    /// </summary>
    public BuildingStatus Status { get; private set; }

    /// <summary>
    /// Damage level assessment (مستوى الضرر)
    /// </summary>
    public DamageLevel? DamageLevel { get; private set; }

    /// <summary>
    /// Total number of property units (عدد الوحدات العقارية)
    /// </summary>
    public int NumberOfPropertyUnits { get; private set; }

    /// <summary>
    /// Number of residential apartments (عدد المقاسم)
    /// </summary>
    public int NumberOfApartments { get; private set; }

    /// <summary>
    /// Number of commercial shops (عدد المحلات)
    /// </summary>
    public int NumberOfShops { get; private set; }

    /// <summary>
    /// Number of floors in the building
    /// </summary>
    public int? NumberOfFloors { get; private set; }

    /// <summary>
    /// Year when building was constructed
    /// </summary>
    public int? YearOfConstruction { get; private set; }

    // ==================== SPATIAL DATA ====================

    /// <summary>
    /// Building geometry in WKT format (Well-Known Text)
    /// Stored as PostGIS geometry in database
    /// </summary>
    public string? BuildingGeometryWkt { get; private set; }

    /// <summary>
    /// GPS latitude coordinate
    /// </summary>
    public decimal? Latitude { get; private set; }

    /// <summary>
    /// GPS longitude coordinate
    /// </summary>
    public decimal? Longitude { get; private set; }

    // ==================== ADDITIONAL INFORMATION ====================

    /// <summary>
    /// Building address or description
    /// </summary>
    public string? Address { get; private set; }

    /// <summary>
    /// Landmark or notable features near the building
    /// </summary>
    public string? Landmark { get; private set; }

    /// <summary>
    /// Additional notes about the building
    /// </summary>
    public string? Notes { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Property units within this building
    /// </summary>
    public virtual ICollection<PropertyUnit> PropertyUnits { get; private set; }

    /// <summary>
    /// Building assignments to field collectors
    /// </summary>
    public virtual ICollection<BuildingAssignment> BuildingAssignments { get; private set; }

    /// <summary>
    /// Surveys conducted for this building
    /// </summary>
    public virtual ICollection<Survey> Surveys { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor (required for materialization)
    /// </summary>
    private Building() : base()
    {
        BuildingId = string.Empty;
        GovernorateCode = string.Empty;
        DistrictCode = string.Empty;
        SubDistrictCode = string.Empty;
        CommunityCode = string.Empty;
        NeighborhoodCode = string.Empty;
        BuildingNumber = string.Empty;
        GovernorateName = string.Empty;
        DistrictName = string.Empty;
        SubDistrictName = string.Empty;
        CommunityName = string.Empty;
        NeighborhoodName = string.Empty;
        PropertyUnits = new List<PropertyUnit>();
        BuildingAssignments = new List<BuildingAssignment>();
        Surveys = new List<Survey>();
        Status = BuildingStatus.Unknown;
    }

    /// <summary>
    /// Create new building with administrative codes
    /// </summary>
    public static Building Create(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        string neighborhoodCode,
        string buildingNumber,
        string governorateName,
        string districtName,
        string subDistrictName,
        string communityName,
        string neighborhoodName,
        BuildingType buildingType,
        Guid createdByUserId)
    {
        var building = new Building
        {
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            SubDistrictCode = subDistrictCode,
            CommunityCode = communityCode,
            NeighborhoodCode = neighborhoodCode,
            BuildingNumber = buildingNumber,
            GovernorateName = governorateName,
            DistrictName = districtName,
            SubDistrictName = subDistrictName,
            CommunityName = communityName,
            NeighborhoodName = neighborhoodName,
            BuildingType = buildingType,
            Status = BuildingStatus.Unknown
        };

        // Generate 17-digit Building ID
        building.BuildingId = $"{governorateCode}{districtCode}{subDistrictCode}" +
                             $"{communityCode}{neighborhoodCode}{buildingNumber}";

        building.MarkAsCreated(createdByUserId);

        return building;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update building status and damage level
    /// </summary>
    public void UpdateStatus(BuildingStatus status, DamageLevel? damageLevel, Guid modifiedByUserId)
    {
        Status = status;
        DamageLevel = damageLevel;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update building unit counts
    /// </summary>
    public void UpdateUnitCounts(int apartments, int shops, Guid modifiedByUserId)
    {
        NumberOfApartments = apartments;
        NumberOfShops = shops;
        NumberOfPropertyUnits = apartments + shops;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set building geometry
    /// </summary>
    public void SetGeometry(string geometryWkt, Guid modifiedByUserId)
    {
        BuildingGeometryWkt = geometryWkt;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set GPS coordinates
    /// </summary>
    public void SetCoordinates(decimal latitude, decimal longitude, Guid modifiedByUserId)
    {
        Latitude = latitude;
        Longitude = longitude;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update building type
    /// </summary>
    public void UpdateBuildingType(BuildingType buildingType, Guid modifiedByUserId)
    {
        BuildingType = buildingType;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update building details
    /// </summary>
    public void UpdateDetails(
        int? numberOfFloors,
        int? yearOfConstruction,
        string? address,
        string? landmark,
        string? notes,
        Guid modifiedByUserId)
    {
        NumberOfFloors = numberOfFloors;
        YearOfConstruction = yearOfConstruction;
        Address = address;
        Landmark = landmark;
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }
}