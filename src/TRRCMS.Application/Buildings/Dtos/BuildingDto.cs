namespace TRRCMS.Application.Buildings.Dtos;

/// <summary>
/// Building Data Transfer Object
/// Used for API responses
/// </summary>
public class BuildingDto
{
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


    /// <summary>
    /// Building type (نوع البناء)
    /// Values: Residential, Commercial, MixedUse, Industrial
    /// </summary>
    public int BuildingType { get; set; }

    /// <summary>
    /// Building status (حالة البناء)
    /// </summary>
    public int Status { get; set; }

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


    /// <summary>
    /// IDs of building documents (photos/PDFs) attached to this building (وثائق البناء)
    /// Use GET /api/v1/building-documents/{id} to retrieve document details.
    /// </summary>
    public List<Guid> BuildingDocumentIds { get; set; } = new();


    /// <summary>
    /// General notes (الوصف العام)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this building has ever been assigned to a field collector
    /// </summary>
    public bool IsAssigned { get; set; }

    /// <summary>
    /// Whether this building is locked (import pipeline will not update it)
    /// </summary>
    public bool IsLocked { get; set; }


    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime? LastModifiedAtUtc { get; set; }
}