using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuilding;

public class GetBuildingQueryHandler : IRequestHandler<GetBuildingQuery, BuildingDto?>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetBuildingQueryHandler(
        IBuildingRepository buildingRepository,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<BuildingDto?> Handle(GetBuildingQuery request, CancellationToken cancellationToken)
    {
        var building = await _buildingRepository.GetByIdAsync(request.Id, cancellationToken);

        if (building == null)
            return null;

        return _mapper.Map<BuildingDto>(building);
    }
}