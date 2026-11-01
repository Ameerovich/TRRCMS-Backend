using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsForMap;

/// <summary>
/// Handler for GetBuildingsForMapQuery
/// Returns lightweight DTOs optimized for map rendering.
/// 
/// Uses a single projected PostGIS query, so only map fields are loaded
/// and assignment state needs no per-building lookups.
/// </summary>
public class GetBuildingsForMapQueryHandler : IRequestHandler<GetBuildingsForMapQuery, List<BuildingMapDto>>
{
    private readonly IBuildingRepository _buildingRepository;

    public GetBuildingsForMapQueryHandler(IBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public Task<List<BuildingMapDto>> Handle(
        GetBuildingsForMapQuery request,
        CancellationToken cancellationToken)
    {
        return _buildingRepository.GetMapBuildingsInBoundingBoxAsync(
            minLatitude: request.SouthWestLat,
            maxLatitude: request.NorthEastLat,
            minLongitude: request.SouthWestLng,
            maxLongitude: request.NorthEastLng,
            buildingType: request.BuildingType,
            status: request.Status,
            maxResults: request.MaxResults,
            includeGeometry: request.IncludeGeometry,
            cancellationToken: cancellationToken);
    }
}
