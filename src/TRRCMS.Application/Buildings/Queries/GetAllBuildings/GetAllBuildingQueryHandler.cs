using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetAllBuildings;

public class GetAllBuildingsQueryHandler : IRequestHandler<GetAllBuildingsQuery, List<BuildingDto>>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetAllBuildingsQueryHandler(
        IBuildingRepository buildingRepository,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<List<BuildingDto>> Handle(GetAllBuildingsQuery request, CancellationToken cancellationToken)
    {
        var buildings = await _buildingRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<BuildingDto>>(buildings);
    }
}