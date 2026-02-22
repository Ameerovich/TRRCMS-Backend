using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Buildings.Queries.GetAllBuildings;

public class GetAllBuildingsQueryHandler : IRequestHandler<GetAllBuildingsQuery, PagedResult<BuildingDto>>
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

    public async Task<PagedResult<BuildingDto>> Handle(GetAllBuildingsQuery request, CancellationToken cancellationToken)
    {
        var buildings = await _buildingRepository.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<BuildingDto>>(buildings);
        return PaginatedList.FromEnumerable(dtos, request.PageNumber, request.PageSize);
    }
}
