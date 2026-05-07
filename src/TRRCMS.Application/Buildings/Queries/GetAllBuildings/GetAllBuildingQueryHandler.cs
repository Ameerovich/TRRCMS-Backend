using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Buildings.Queries.GetAllBuildings;

public class GetAllBuildingsQueryHandler : IRequestHandler<GetAllBuildingsQuery, PagedResult<BuildingDto>>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICommunityRepository _communityRepository;
    private readonly IMapper _mapper;

    public GetAllBuildingsQueryHandler(
        IBuildingRepository buildingRepository,
        ICommunityRepository communityRepository,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _communityRepository = communityRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<BuildingDto>> Handle(GetAllBuildingsQuery request, CancellationToken cancellationToken)
    {
        var buildings = await _buildingRepository.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<BuildingDto>>(buildings);
        await BuildingDtoEnricher.EnrichCommunityPCodesAsync(dtos, _communityRepository, cancellationToken);
        return PaginatedList.FromEnumerable(dtos, request.PageNumber, request.PageSize);
    }
}
