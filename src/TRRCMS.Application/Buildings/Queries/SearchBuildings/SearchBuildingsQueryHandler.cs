using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.SearchBuildings;

/// <summary>
/// Handler for SearchBuildingsQuery
/// </summary>
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
        // Validate pagination
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : (request.PageSize > 100 ? 100 : request.PageSize);

        // Get filtered and paginated buildings
        var (buildings, totalCount) = await _buildingRepository.SearchBuildingsAsync(
            governorateCode: request.GovernorateCode,
            districtCode: request.DistrictCode,
            subDistrictCode: request.SubDistrictCode,
            communityCode: request.CommunityCode,
            neighborhoodCode: request.NeighborhoodCode,
            buildingId: request.BuildingId,
            buildingNumber: request.BuildingNumber,
            address: null,          // Not used in current frontend
            latitude: null,         // Not used for search
            longitude: null,        // Not used for search
            radiusMeters: null,     // Not used in current frontend
            status: request.Status,
            buildingType: request.BuildingType,
            damageLevel: null,      // Not used in current frontend
            page: page,
            pageSize: pageSize,
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            cancellationToken: cancellationToken);

        // Map to DTOs
        var buildingDtos = _mapper.Map<List<BuildingDto>>(buildings);

        return new SearchBuildingsResponse
        {
            Buildings = buildingDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}