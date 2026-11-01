using MediatR;
using TRRCMS.Application.Buildings.Dtos;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingMapGrid;

/// <summary>
/// Query to count buildings per grid cell within a bounding box.
/// Lets the map summarize any number of buildings at low zoom levels.
/// </summary>
public class GetBuildingMapGridQuery : IRequest<List<BuildingMapGridCellDto>>
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
    /// Grid cell size in degrees
    /// </summary>
    public double CellSizeDegrees { get; set; }
}
