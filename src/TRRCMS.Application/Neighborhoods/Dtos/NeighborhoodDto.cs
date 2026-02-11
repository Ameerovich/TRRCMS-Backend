namespace TRRCMS.Application.Neighborhoods.Dtos;

/// <summary>
/// Neighborhood reference data DTO.
/// Used by frontend for map navigation, boundary rendering, and dropdown population.
/// </summary>
public class NeighborhoodDto
{
    /// <summary>
    /// Database ID (GUID)
    /// </summary>
    public Guid Id { get; set; }

    // ==================== CODES ====================

    /// <summary>
    /// Governorate code (محافظة) — 2 digits
    /// </summary>
    public string GovernorateCode { get; set; } = string.Empty;

    /// <summary>
    /// District code (مدينة) — 2 digits
    /// </summary>
    public string DistrictCode { get; set; } = string.Empty;

    /// <summary>
    /// Sub-district code (بلدة) — 2 digits
    /// </summary>
    public string SubDistrictCode { get; set; } = string.Empty;

    /// <summary>
    /// Community code (قرية) — 3 digits
    /// </summary>
    public string CommunityCode { get; set; } = string.Empty;

    /// <summary>
    /// Neighborhood code (حي) — 3 digits
    /// </summary>
    public string NeighborhoodCode { get; set; } = string.Empty;

    /// <summary>
    /// Full composite code: GGDDSSCCCCNNN (12 digits)
    /// Matches the first 12 characters of Building.BuildingId
    /// </summary>
    public string FullCode { get; set; } = string.Empty;

    // ==================== NAMES ====================

    /// <summary>
    /// Neighborhood name in Arabic (الاسم بالعربية)
    /// </summary>
    public string NameArabic { get; set; } = string.Empty;

    /// <summary>
    /// Neighborhood name in English
    /// </summary>
    public string? NameEnglish { get; set; }

    // ==================== SPATIAL DATA ====================

    /// <summary>
    /// Center latitude for "fly-to" map navigation
    /// </summary>
    public decimal CenterLatitude { get; set; }

    /// <summary>
    /// Center longitude for "fly-to" map navigation
    /// </summary>
    public decimal CenterLongitude { get; set; }

    /// <summary>
    /// Boundary polygon in WKT format for rendering on map.
    /// Null if no boundary polygon is defined.
    /// WKT coordinate order: longitude latitude (X Y).
    /// </summary>
    public string? BoundaryWkt { get; set; }

    /// <summary>
    /// Approximate area in square kilometers
    /// </summary>
    public double? AreaSquareKm { get; set; }

    /// <summary>
    /// Suggested map zoom level
    /// </summary>
    public int ZoomLevel { get; set; }

    /// <summary>
    /// Whether this neighborhood is active
    /// </summary>
    public bool IsActive { get; set; }
}
