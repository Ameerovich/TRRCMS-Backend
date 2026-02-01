using TRRCMS.Application.Buildings.Dtos;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsInPolygon;

/// <summary>
/// Response for polygon-based building search
/// </summary>
public class GetBuildingsInPolygonResponse
{
    /// <summary>
    /// Buildings found within the polygon
    /// </summary>
    public List<BuildingInPolygonDto> Buildings { get; set; } = new();

    /// <summary>
    /// Total count of buildings in polygon (before pagination)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total pages available
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Polygon area in square meters (calculated by PostGIS)
    /// </summary>
    public double? PolygonAreaSquareMeters { get; set; }

    /// <summary>
    /// The WKT polygon that was used for the search
    /// </summary>
    public string PolygonWkt { get; set; } = string.Empty;
}

/// <summary>
/// Lightweight building DTO optimized for polygon/map queries
/// </summary>
public class BuildingInPolygonDto
{
    /// <summary>
    /// Building GUID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Building code (stored format: 17 digits)
    /// </summary>
    public string BuildingId { get; set; } = string.Empty;

    /// <summary>
    /// Formatted building code for display
    /// Format: GG-DD-SS-CCC-NNN-BBBBB
    /// </summary>
    public string BuildingIdFormatted { get; set; } = string.Empty;

    /// <summary>
    /// GPS latitude (center point)
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// GPS longitude (center point)
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Building type
    /// </summary>
    public string BuildingType { get; set; } = string.Empty;

    /// <summary>
    /// Building status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Damage level (if assessed)
    /// </summary>
    public string? DamageLevel { get; set; }

    /// <summary>
    /// Number of property units
    /// </summary>
    public int NumberOfPropertyUnits { get; set; }

    /// <summary>
    /// Building geometry in WKT (only if IncludeFullDetails=true)
    /// </summary>
    public string? BuildingGeometryWkt { get; set; }

    /// <summary>
    /// Neighborhood name (Arabic)
    /// </summary>
    public string NeighborhoodName { get; set; } = string.Empty;

    /// <summary>
    /// Community name (Arabic)
    /// </summary>
    public string CommunityName { get; set; } = string.Empty;

    // ==================== FULL DETAILS (when requested) ====================

    /// <summary>
    /// Full building details (only populated when IncludeFullDetails=true)
    /// </summary>
    public BuildingDto? FullDetails { get; set; }
}
