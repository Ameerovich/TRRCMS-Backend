using MediatR;
using TRRCMS.Application.Streets.Dtos;

namespace TRRCMS.Application.Streets.Queries.GetStreetsForMap;

/// <summary>
/// Get streets intersecting a bounding box for map rendering.
/// </summary>
public record GetStreetsForMapQuery : IRequest<List<StreetMapDto>>
{
    public decimal NorthEastLat { get; init; }
    public decimal NorthEastLng { get; init; }
    public decimal SouthWestLat { get; init; }
    public decimal SouthWestLng { get; init; }
}
