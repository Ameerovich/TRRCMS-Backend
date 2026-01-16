using MediatR;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Queries.SearchBuildings;

/// <summary>
/// Search buildings with multiple filter criteria
/// Supports administrative hierarchy, spatial, and attribute filters
/// </summary>
public class SearchBuildingsQuery : IRequest<SearchBuildingsResponse>
{
    // Administrative hierarchy filters
    public string? GovernorateCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? SubDistrictCode { get; set; }
    public string? CommunityCode { get; set; }
    public string? NeighborhoodCode { get; set; }

    // Direct identifiers
    public string? BuildingId { get; set; }
    public string? BuildingNumber { get; set; }

    // Text search
    public string? Address { get; set; }

    // Spatial filters (for map-based search)
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? RadiusMeters { get; set; }

    // Attribute filters
    public BuildingStatus? Status { get; set; }
    public BuildingType? BuildingType { get; set; }
    public DamageLevel? DamageLevel { get; set; }

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // Sorting
    public string? SortBy { get; set; } // "buildingId", "createdDate"
    public bool SortDescending { get; set; } = false;
}