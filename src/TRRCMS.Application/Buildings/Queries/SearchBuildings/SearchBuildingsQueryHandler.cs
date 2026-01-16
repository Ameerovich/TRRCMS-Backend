using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.SearchBuildings;

public class SearchBuildingsQueryHandler : IRequestHandler<SearchBuildingsQuery, SearchBuildingsResponse>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public SearchBuildingsQueryHandler(
        IBuildingRepository buildingRepository,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<SearchBuildingsResponse> Handle(
        SearchBuildingsQuery request,
        CancellationToken cancellationToken)
    {
        // Get filtered and paginated buildings
        var (buildings, totalCount) = await _buildingRepository.SearchBuildingsAsync(
            governorateCode: request.GovernorateCode,
            districtCode: request.DistrictCode,
            subDistrictCode: request.SubDistrictCode,
            communityCode: request.CommunityCode,
            neighborhoodCode: request.NeighborhoodCode,
            buildingId: request.BuildingId,
            buildingNumber: request.BuildingNumber,
            address: request.Address,
            latitude: request.Latitude,
            longitude: request.Longitude,
            radiusMeters: request.RadiusMeters,
            status: request.Status,
            buildingType: request.BuildingType,
            damageLevel: request.DamageLevel,
            page: request.Page,
            pageSize: request.PageSize,
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            cancellationToken: cancellationToken);

        // Map to DTOs
        var buildingDtos = _mapper.Map<List<BuildingDto>>(buildings);

        return new SearchBuildingsResponse
        {
            Buildings = buildingDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}