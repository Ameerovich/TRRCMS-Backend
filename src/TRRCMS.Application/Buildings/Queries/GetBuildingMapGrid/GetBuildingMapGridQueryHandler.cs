using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingMapGrid;

/// <summary>
/// Handler for GetBuildingMapGridQuery
/// </summary>
public class GetBuildingMapGridQueryHandler : IRequestHandler<GetBuildingMapGridQuery, List<BuildingMapGridCellDto>>
{
    private readonly IBuildingRepository _buildingRepository;

    public GetBuildingMapGridQueryHandler(IBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public Task<List<BuildingMapGridCellDto>> Handle(
        GetBuildingMapGridQuery request,
        CancellationToken cancellationToken)
    {
        return _buildingRepository.GetMapGridInBoundingBoxAsync(
            minLatitude: request.SouthWestLat,
            maxLatitude: request.NorthEastLat,
            minLongitude: request.SouthWestLng,
            maxLongitude: request.NorthEastLng,
            cellSizeDegrees: request.CellSizeDegrees,
            cancellationToken: cancellationToken);
    }
}
