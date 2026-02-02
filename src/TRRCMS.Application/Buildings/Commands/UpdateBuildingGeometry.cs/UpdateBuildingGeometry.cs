using MediatR;
using TRRCMS.Application.Buildings.Dtos;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuildingGeometry;

/// <summary>
/// Command to update building geometry and coordinates
/// UC-000: Manage Building Data
/// </summary>
public class UpdateBuildingGeometryCommand : IRequest<BuildingDto>
{
    /// <summary>
    /// Building ID to update
    /// </summary>
    public Guid BuildingId { get; set; }

    /// <summary>
    /// Building geometry in WKT (Well-Known Text) format
    /// Example: "POLYGON((36.2 35.8, 36.3 35.8, 36.3 35.9, 36.2 35.9, 36.2 35.8))"
    /// </summary>
    public string? GeometryWkt { get; set; }

    /// <summary>
    /// GPS Latitude coordinate (decimal degrees)
    /// Syria bounds: 32.0 to 37.5
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// GPS Longitude coordinate (decimal degrees)
    /// Syria bounds: 36.0 to 42.5
    /// </summary>
    public decimal? Longitude { get; set; }
}