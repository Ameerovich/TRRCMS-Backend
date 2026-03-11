using MediatR;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Queries.GetLandmarksForMap;

/// <summary>
/// Get landmarks within a bounding box for map rendering.
/// </summary>
public record GetLandmarksForMapQuery : IRequest<List<LandmarkMapDto>>
{
    public decimal NorthEastLat { get; init; }
    public decimal NorthEastLng { get; init; }
    public decimal SouthWestLat { get; init; }
    public decimal SouthWestLng { get; init; }

    /// <summary>Optional filter by landmark type</summary>
    public int? Type { get; init; }
}
