using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetBuildingsForAssignment;

/// <summary>
/// Query to get buildings available for assignment.
/// Supports administrative hierarchy filters, text search, radius search, and polygon search.
/// </summary>
public record GetBuildingsForAssignmentQuery : IRequest<BuildingsForAssignmentPagedResult>
{
    /// <summary>
    /// Filter by governorate code (محافظة)
    /// </summary>
    public string? GovernorateCode { get; init; }
    
    /// <summary>
    /// Filter by district code (مدينة)
    /// </summary>
    public string? DistrictCode { get; init; }
    
    /// <summary>
    /// Filter by sub-district code (بلدة)
    /// </summary>
    public string? SubDistrictCode { get; init; }
    
    /// <summary>
    /// Filter by community code (قرية)
    /// </summary>
    public string? CommunityCode { get; init; }
    
    /// <summary>
    /// Filter by neighborhood code (حي)
    /// </summary>
    public string? NeighborhoodCode { get; init; }
    
    /// <summary>
    /// Search by building code (partial match)
    /// </summary>
    public string? BuildingCode { get; init; }

    /// <summary>
    /// Filter by building type
    /// </summary>
    public BuildingType? BuildingType { get; init; }
    
    /// <summary>
    /// Filter by building status
    /// </summary>
    public BuildingStatus? BuildingStatus { get; init; }
    
    /// <summary>
    /// Filter by assignment status:
    /// - null: All buildings
    /// - true: Only buildings with active assignments
    /// - false: Only buildings without active assignments
    /// </summary>
    public bool? HasActiveAssignment { get; init; }
    
    /// <summary>
    /// Center latitude for radius-based spatial search
    /// </summary>
    public decimal? Latitude { get; init; }
    
    /// <summary>
    /// Center longitude for radius-based spatial search
    /// </summary>
    public decimal? Longitude { get; init; }
    
    /// <summary>
    /// Radius in meters for radius-based spatial search
    /// </summary>
    public int? RadiusMeters { get; init; }
    
    /// <summary>
    /// Polygon geometry in WKT (Well-Known Text) format for spatial search
    /// Example: "POLYGON((37.13 36.20, 37.14 36.20, 37.14 36.21, 37.13 36.21, 37.13 36.20))"
    /// Coordinates are in longitude-latitude order. First and last coordinate must be identical.
    /// </summary>
    public string? PolygonWkt { get; init; }
    
    /// <summary>
    /// Alternative: Array of coordinates for polygon vertices
    /// Format: [[lng1, lat1], [lng2, lat2], ...]
    /// If provided, will be converted to WKT internally
    /// Polygon will be auto-closed if first != last coordinate
    /// </summary>
    public double[][]? Coordinates { get; init; }
    
    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int Page { get; init; } = 1;
    
    /// <summary>
    /// Items per page (default: 20, max: 1000 for polygon search)
    /// </summary>
    public int PageSize { get; init; } = 20;
    
    /// <summary>
    /// Sort field (e.g., "buildingCode", "address", "createdDate")
    /// </summary>
    public string? SortBy { get; init; }
    
    /// <summary>
    /// Sort direction (default: false = ascending)
    /// </summary>
    public bool SortDescending { get; init; } = false;
}
