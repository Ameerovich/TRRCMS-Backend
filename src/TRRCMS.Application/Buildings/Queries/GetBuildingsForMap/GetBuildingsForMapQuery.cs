using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsForMap;

/// <summary>
/// Query to get buildings within a bounding box for map display
/// Returns lightweight DTOs optimized for performance
/// UC-000: Manage Building Data - Map View
/// </summary>
public class GetBuildingsForMapQuery : IRequest<List<BuildingMapDto>>
{
    /// <summary>
    /// Northeast corner latitude of bounding box
    /// </summary>
    public decimal NorthEastLat { get; set; }

    /// <summary>
    /// Northeast corner longitude of bounding box
    /// </summary>
    public decimal NorthEastLng { get; set; }

    /// <summary>
    /// Southwest corner latitude of bounding box
    /// </summary>
    public decimal SouthWestLat { get; set; }

    /// <summary>
    /// Southwest corner longitude of bounding box
    /// </summary>
    public decimal SouthWestLng { get; set; }

    /// <summary>
    /// Optional: Filter by building status
    /// </summary>
    public BuildingStatus? Status { get; set; }

    /// <summary>
    /// Optional: Filter by building type
    /// </summary>
    public BuildingType? BuildingType { get; set; }

    /// <summary>
    /// Optional: Filter by damage level
    /// </summary>
    public DamageLevel? DamageLevel { get; set; }

    /// <summary>
    /// Maximum number of buildings to return (prevent overload)
    /// Default: 10000
    /// </summary>
    public int MaxResults { get; set; } = 10000;
}