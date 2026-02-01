using MediatR;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsInPolygon;

/// <summary>
/// Query to get buildings within a polygon area
/// Uses PostGIS ST_Within for spatial filtering
/// البحث عن المباني داخل مضلع جغرافي
/// </summary>
public class GetBuildingsInPolygonQuery : IRequest<GetBuildingsInPolygonResponse>
{
    /// <summary>
    /// Polygon geometry in WKT (Well-Known Text) format
    /// Example: "POLYGON((37.13 36.20, 37.14 36.20, 37.14 36.21, 37.13 36.21, 37.13 36.20))"
    /// Note: First and last coordinate must be identical to close the polygon
    /// </summary>
    public string PolygonWkt { get; set; } = string.Empty;

    /// <summary>
    /// Alternative: Array of coordinates for polygon vertices
    /// Format: [[lng1, lat1], [lng2, lat2], ...]
    /// If provided, will be converted to WKT internally
    /// </summary>
    public double[][]? Coordinates { get; set; }

    // ==================== OPTIONAL FILTERS ====================

    /// <summary>
    /// Filter by building type (نوع البناء)
    /// </summary>
    public BuildingType? BuildingType { get; set; }

    /// <summary>
    /// Filter by building status (حالة البناء)
    /// </summary>
    public BuildingStatus? Status { get; set; }

    /// <summary>
    /// Filter by damage level (مستوى الضرر)
    /// </summary>
    public DamageLevel? DamageLevel { get; set; }

    // ==================== PAGINATION ====================

    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Items per page (default: 100, max: 1000 for map display)
    /// </summary>
    public int PageSize { get; set; } = 100;

    // ==================== OPTIONS ====================

    /// <summary>
    /// Include full building details (default: false for performance)
    /// When false, returns lightweight data optimized for map markers
    /// </summary>
    public bool IncludeFullDetails { get; set; } = false;
}
