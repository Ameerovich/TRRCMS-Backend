using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsForMap;

public class GetBuildingsForMapQueryHandler : IRequestHandler<GetBuildingsForMapQuery, List<BuildingMapDto>>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetBuildingsForMapQueryHandler(
        IBuildingRepository buildingRepository,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<List<BuildingMapDto>> Handle(
        GetBuildingsForMapQuery request,
        CancellationToken cancellationToken)
    {
        // Get all buildings (we'll filter in-memory for now)
        // Alternative: Add GetBuildingsInBoundingBoxAsync to repository for better performance
        var allBuildings = await _buildingRepository.GetAllAsync(cancellationToken);

        // Filter to buildings within bounding box
        var buildingsInBox = allBuildings
            .Where(b =>
                b.Latitude != null &&
                b.Longitude != null &&
                b.Latitude >= request.SouthWestLat &&
                b.Latitude <= request.NorthEastLat &&
                b.Longitude >= request.SouthWestLng &&
                b.Longitude <= request.NorthEastLng);

        // Apply optional status filter
        if (request.Status.HasValue)
        {
            buildingsInBox = buildingsInBox.Where(b => b.Status == request.Status.Value);
        }

        // Apply optional building type filter
        if (request.BuildingType.HasValue)
        {
            buildingsInBox = buildingsInBox.Where(b => b.BuildingType == request.BuildingType.Value);
        }

        // Apply optional damage level filter
        if (request.DamageLevel.HasValue)
        {
            buildingsInBox = buildingsInBox.Where(b => b.DamageLevel == request.DamageLevel.Value);
        }

        // Limit results to prevent overload
        var limitedBuildings = buildingsInBox.Take(request.MaxResults);

        // Map to lightweight DTOs
        var buildingMapDtos = limitedBuildings.Select(b => new BuildingMapDto
        {
            Id = b.Id,
            BuildingId = b.BuildingId,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            Status = b.Status.ToString(),
            BuildingType = b.BuildingType.ToString(),
            DamageLevel = b.DamageLevel?.ToString(),
            Address = b.Address,
            NumberOfPropertyUnits = b.NumberOfPropertyUnits,
            NeighborhoodName = b.NeighborhoodName
        }).ToList();

        return buildingMapDtos;
    }
}