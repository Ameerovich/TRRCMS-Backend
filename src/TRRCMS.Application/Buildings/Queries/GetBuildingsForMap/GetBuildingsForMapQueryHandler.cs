using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsForMap;

/// <summary>
/// Handler for GetBuildingsForMapQuery
/// Returns lightweight DTOs optimized for map rendering.
/// 
/// Uses GetBuildingsInBoundingBoxAsync() instead of GetAllAsync()
/// to avoid loading all buildings into memory.
/// </summary>
public class GetBuildingsForMapQueryHandler : IRequestHandler<GetBuildingsForMapQuery, List<BuildingMapDto>>
{
    private readonly IBuildingRepository _buildingRepository;

    public GetBuildingsForMapQueryHandler(IBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public async Task<List<BuildingMapDto>> Handle(
        GetBuildingsForMapQuery request,
        CancellationToken cancellationToken)
    {
        // Use PostGIS bounding box query instead of loading ALL buildings
        var buildings = await _buildingRepository.GetBuildingsInBoundingBoxAsync(
            minLatitude: request.SouthWestLat,
            maxLatitude: request.NorthEastLat,
            minLongitude: request.SouthWestLng,
            maxLongitude: request.NorthEastLng,
            buildingType: request.BuildingType,
            status: request.Status,
            maxResults: request.MaxResults,
            cancellationToken: cancellationToken);

        // Map to lightweight DTOs
        var buildingMapDtos = buildings.Select(b => new BuildingMapDto
        {
            Id = b.Id,
            BuildingId = b.BuildingId,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            BuildingGeometryWkt = b.BuildingGeometryWkt,
            Status = b.Status.ToString(),
            BuildingType = b.BuildingType.ToString(),
            NumberOfPropertyUnits = b.NumberOfPropertyUnits,
            NumberOfApartments = b.NumberOfApartments,
            NumberOfShops = b.NumberOfShops
        }).ToList();

        return buildingMapDtos;
    }
}
