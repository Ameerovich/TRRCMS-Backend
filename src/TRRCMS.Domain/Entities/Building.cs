using NetTopologySuite.Geometries;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Building entity - represents physical buildings in the system.
/// Uses PostGIS geometry for spatial operations.
/// Follows DDD principles with encapsulated state and domain methods.
/// </summary>
public class Building : BaseAuditableEntity
{
    /// <summary>
    /// Unique building identifier (رمز البناء)
    /// Stored format: GGDDSSCCNCNNBBBBB (17 digits, no dashes)
    /// Display format: GG-DD-SS-CCC-NNN-BBBBB (with dashes, computed in DTO)
    /// Auto-generated from administrative codes
    /// </summary>
    public string BuildingId { get; private set; }
    /// <summary>
    /// Governorate code (محافظة) - 2 digits
    /// </summary>
    public string GovernorateCode { get; private set; }

    /// <summary>
    /// District code (مدينة) - 2 digits
    /// </summary>
    public string DistrictCode { get; private set; }

    /// <summary>
    /// Sub-district code (بلدة) - 2 digits
    /// </summary>
    public string SubDistrictCode { get; private set; }

    /// <summary>
    /// Community code (قرية) - 3 digits
    /// </summary>
    public string CommunityCode { get; private set; }

    /// <summary>
    /// Neighborhood code (حي) - 3 digits
    /// </summary>
    public string NeighborhoodCode { get; private set; }

    /// <summary>
    /// Building number within neighborhood (رقم البناء) - 5 digits
    /// </summary>
    public string BuildingNumber { get; private set; }
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
    /// <summary>
    /// Building type classification (نوع البناء)
    /// </summary>
    public BuildingType BuildingType { get; private set; }

    /// <summary>
    /// Building status - physical condition (حالة البناء)
    /// </summary>
    public BuildingStatus Status { get; private set; }

    /// <summary>
    /// Total number of property units (عدد الوحدات)
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
    /// Building geometry stored as PostGIS native type
    /// Uses SRID 4326 (WGS84 - GPS coordinate system)
    /// </summary>
    public Geometry? BuildingGeometry { get; private set; }

    /// <summary>
    /// Building geometry as WKT string (computed property, not stored)
    /// Used for API responses - maintains backwards compatibility
    /// </summary>
    public string? BuildingGeometryWkt => BuildingGeometry?.AsText();

    /// <summary>
    /// GPS latitude coordinate (center point)
    /// </summary>
    public decimal? Latitude { get; private set; }

    /// <summary>
    /// GPS longitude coordinate (center point)
    /// </summary>
    public decimal? Longitude { get; private set; }
    /// <summary>
    /// Additional notes / General description (الوصف العام)
    /// </summary>
    public string? Notes { get; private set; }
    public virtual ICollection<BuildingDocument> BuildingDocuments { get; private set; }
    public virtual ICollection<PropertyUnit> PropertyUnits { get; private set; }
    public virtual ICollection<BuildingAssignment> BuildingAssignments { get; private set; }
    public virtual ICollection<Survey> Surveys { get; private set; }
    /// <summary>
    /// EF Core constructor
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
        BuildingDocuments = new List<BuildingDocument>();
        PropertyUnits = new List<PropertyUnit>();
        BuildingAssignments = new List<BuildingAssignment>();
        Surveys = new List<Survey>();
        Status = BuildingStatus.Unknown;
    }
    /// <summary>
    /// Create new building (Factory Method - DDD pattern)
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
        BuildingStatus status,
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
            Status = status
        };

        // Generate Building ID (رمز البناء) - stored without dashes
        building.BuildingId = $"{governorateCode}{districtCode}{subDistrictCode}" +
                             $"{communityCode}{neighborhoodCode}{buildingNumber}";

        building.MarkAsCreated(createdByUserId);

        return building;
    }
    /// <summary>
    /// Update building status
    /// </summary>
    public void UpdateStatus(BuildingStatus status, Guid modifiedByUserId)
    {
        Status = status;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update building unit counts
    /// </summary>
    public void UpdateUnitCounts(int propertyUnits, int apartments, int shops, Guid modifiedByUserId)
    {
        NumberOfPropertyUnits = propertyUnits;
        NumberOfApartments = apartments;
        NumberOfShops = shops;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set building geometry (pre-parsed by IGeometryConverter).
    /// Auto-computes centroid for polygon geometries.
    /// </summary>
    public void SetGeometry(Geometry? geometry, Guid modifiedByUserId)
    {
        BuildingGeometry = geometry;

        // Auto-compute centroid from polygon so Latitude/Longitude are never null
        // when a polygon geometry is provided
        if (geometry is Polygon or MultiPolygon)
        {
            var centroid = geometry.Centroid;
            Latitude = (decimal)centroid.Y;
            Longitude = (decimal)centroid.X;
        }

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set GPS coordinates.
    /// Optionally accepts a pre-built Point (from IGeometryConverter) to set as geometry
    /// when no geometry exists yet.
    /// </summary>
    public void SetCoordinates(decimal latitude, decimal longitude, Guid modifiedByUserId, Point? fallbackPoint = null)
    {
        Latitude = latitude;
        Longitude = longitude;

        // Use provided point geometry if no geometry exists
        if (BuildingGeometry == null && fallbackPoint != null)
        {
            BuildingGeometry = fallbackPoint;
        }

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
        string? notes,
        Guid modifiedByUserId)
    {
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update building with field survey data from .uhc import.
    /// Updates all building attributes EXCEPT BuildingId, admin codes, geometry, and coordinates.
    /// Used by CommitService when an existing building is found during import commit.
    /// </summary>
    public void UpdateFromFieldSurvey(
        BuildingType buildingType,
        BuildingStatus status,
        int numberOfPropertyUnits,
        int numberOfApartments,
        int numberOfShops,
        string? notes,
        Guid modifiedByUserId)
    {
        BuildingType = buildingType;
        Status = status;
        NumberOfPropertyUnits = numberOfPropertyUnits;
        NumberOfApartments = numberOfApartments;
        NumberOfShops = numberOfShops;
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update building administrative codes and regenerate BuildingId
    /// For office survey workflow to allow editing building code (17 digits)
    /// </summary>
    public void UpdateAdministrativeCodes(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        string neighborhoodCode,
        string buildingNumber,
        string? governorateName,
        string? districtName,
        string? subDistrictName,
        string? communityName,
        string? neighborhoodName,
        Guid modifiedByUserId)
    {
        GovernorateCode = governorateCode;
        DistrictCode = districtCode;
        SubDistrictCode = subDistrictCode;
        CommunityCode = communityCode;
        NeighborhoodCode = neighborhoodCode;
        BuildingNumber = buildingNumber;

        // Only update names if provided
        if (governorateName != null) GovernorateName = governorateName;
        if (districtName != null) DistrictName = districtName;
        if (subDistrictName != null) SubDistrictName = subDistrictName;
        if (communityName != null) CommunityName = communityName;
        if (neighborhoodName != null) NeighborhoodName = neighborhoodName;

        // Regenerate Building ID (رمز البناء) - stored without dashes
        BuildingId = $"{governorateCode}{districtCode}{subDistrictCode}" +
                     $"{communityCode}{neighborhoodCode}{buildingNumber}";

        MarkAsModified(modifiedByUserId);
    }
}
