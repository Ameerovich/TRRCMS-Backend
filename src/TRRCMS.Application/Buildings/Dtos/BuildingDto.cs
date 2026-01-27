namespace TRRCMS.Application.Buildings.Dtos;

/// <summary>
/// Building Data Transfer Object
/// Used for API responses
/// </summary>
public class BuildingDto
{
    // ==================== IDENTIFIERS ====================

    /// <summary>
    /// Database ID (GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Business Building ID stored in database (رمز البناء)
    /// Format: GGDDSSCCNCNNBBBBB (17 digits, no dashes)
    /// </summary>
    public string BuildingId { get; set; } = string.Empty;

    /// <summary>
    /// Formatted Building ID for display (رمز البناء)
    /// Format: GG-DD-SS-CCC-NNN-BBBBB (with dashes)
    /// </summary>
    public string BuildingIdFormatted => FormatBuildingId(BuildingId);

    private static string FormatBuildingId(string buildingId)
    {
        if (string.IsNullOrEmpty(buildingId) || buildingId.Length != 17)
            return buildingId;

        // Format: GG-DD-SS-CCC-NNN-BBBBB
        return $"{buildingId[..2]}-{buildingId[2..4]}-{buildingId[4..6]}-" +
               $"{buildingId[6..9]}-{buildingId[9..12]}-{buildingId[12..17]}";
    }

    // ==================== ADMINISTRATIVE CODES ====================

    /// <summary>
    /// Governorate code (محافظة)
    /// </summary>
    public string GovernorateCode { get; set; } = string.Empty;

    /// <summary>
    /// District code (مدينة)
    /// </summary>
    public string DistrictCode { get; set; } = string.Empty;

    /// <summary>
    /// Sub-district code (بلدة)
    /// </summary>
    public string SubDistrictCode { get; set; } = string.Empty;

    /// <summary>
    /// Community code (قرية)
    /// </summary>
    public string CommunityCode { get; set; } = string.Empty;

    /// <summary>
    /// Neighborhood code (حي)
    /// </summary>
    public string NeighborhoodCode { get; set; } = string.Empty;

    /// <summary>
    /// Building number (رقم البناء)
    /// </summary>
    public string BuildingNumber { get; set; } = string.Empty;

    // ==================== LOCATION NAMES (ARABIC) ====================

    /// <summary>
    /// Governorate name in Arabic
    /// </summary>
    public string GovernorateName { get; set; } = string.Empty;

    /// <summary>
    /// District name in Arabic
    /// </summary>
    public string DistrictName { get; set; } = string.Empty;

    /// <summary>
    /// Sub-district name in Arabic
    /// </summary>
    public string SubDistrictName { get; set; } = string.Empty;

    /// <summary>
    /// Community name in Arabic
    /// </summary>
    public string CommunityName { get; set; } = string.Empty;

    /// <summary>
    /// Neighborhood name in Arabic
    /// </summary>
    public string NeighborhoodName { get; set; } = string.Empty;

    // ==================== BUILDING ATTRIBUTES ====================

    /// <summary>
    /// Building type (نوع البناء)
    /// Values: Residential, Commercial, MixedUse, Industrial
    /// </summary>
    public string BuildingType { get; set; } = string.Empty;

    /// <summary>
    /// Building status (حالة البناء)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Damage level (مستوى الضرر)
    /// </summary>
    public string? DamageLevel { get; set; }

    /// <summary>
    /// Number of property units (عدد الوحدات)
    /// </summary>
    public int NumberOfPropertyUnits { get; set; }

    /// <summary>
    /// Number of apartments (عدد المقاسم)
    /// </summary>
    public int NumberOfApartments { get; set; }

    /// <summary>
    /// Number of shops (عدد المحلات)
    /// </summary>
    public int NumberOfShops { get; set; }

    /// <summary>
    /// Number of floors
    /// </summary>
    public int? NumberOfFloors { get; set; }

    /// <summary>
    /// Year of construction
    /// </summary>
    public int? YearOfConstruction { get; set; }

    // ==================== SPATIAL DATA ====================

    /// <summary>
    /// GPS latitude coordinate
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// GPS longitude coordinate
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Building geometry in WKT format
    /// </summary>
    public string? BuildingGeometryWkt { get; set; }

    // ==================== ADDITIONAL INFORMATION ====================

    /// <summary>
    /// Building address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Landmark near the building
    /// </summary>
    public string? Landmark { get; set; }

    /// <summary>
    /// Location description (وصف الموقع)
    /// </summary>
    public string? LocationDescription { get; set; }

    /// <summary>
    /// General notes (الوصف العام)
    /// </summary>
    public string? Notes { get; set; }

    // ==================== AUDIT ====================

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime? LastModifiedAtUtc { get; set; }
}