using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuilding;

public class GetBuildingQueryHandler : IRequestHandler<GetBuildingQuery, BuildingDto?>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICommunityRepository _communityRepository;
    private readonly IMapper _mapper;

    public GetBuildingQueryHandler(
        IBuildingRepository buildingRepository,
        ICommunityRepository communityRepository,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _communityRepository = communityRepository;
        _mapper = mapper;
    }

    public async Task<BuildingDto?> Handle(GetBuildingQuery request, CancellationToken cancellationToken)
    {
        var building = await _buildingRepository.GetByIdAsync(request.Id, cancellationToken);

        if (building == null)
            return null;

        var dto = _mapper.Map<BuildingDto>(building);
        await BuildingDtoEnricher.EnrichCommunityPCodesAsync(dto, _communityRepository, cancellationToken);
        return dto;
    }
}