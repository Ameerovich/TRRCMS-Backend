namespace TRRCMS.Application.Buildings.Dtos;

/// <summary>
/// Building count for one grid cell, used to summarize the map at low zoom levels
/// </summary>
public class BuildingMapGridCellDto
{
    /// <summary>
    /// Average latitude of the buildings in the cell
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Average longitude of the buildings in the cell
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Number of buildings in the cell
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Number of buildings in the cell with an active field assignment
    /// </summary>
    public int AssignedCount { get; set; }
}
