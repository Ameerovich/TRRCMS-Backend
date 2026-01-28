using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;

/// <summary>
/// Handler for GetAllPropertyUnitsQuery
/// </summary>
public class GetAllPropertyUnitsQueryHandler : IRequestHandler<GetAllPropertyUnitsQuery, List<PropertyUnitDto>>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetAllPropertyUnitsQueryHandler(
        IPropertyUnitRepository propertyUnitRepository,
        IBuildingRepository buildingRepository,
        IMapper mapper)
    {
        _propertyUnitRepository = propertyUnitRepository;
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<List<PropertyUnitDto>> Handle(GetAllPropertyUnitsQuery request, CancellationToken cancellationToken)
    {
        var propertyUnits = await _propertyUnitRepository.GetAllAsync(cancellationToken);

        // Get all buildings for DTO enrichment
        var buildingIds = propertyUnits.Select(p => p.BuildingId).Distinct();
        var buildingDict = new Dictionary<Guid, string>();

        foreach (var buildingId in buildingIds)
        {
            var building = await _buildingRepository.GetByIdAsync(buildingId, cancellationToken);
            if (building != null)
            {
                buildingDict[buildingId] = building.BuildingNumber;
            }
        }

        // Map to DTOs
        var result = propertyUnits.Select(unit =>
        {
            var dto = _mapper.Map<PropertyUnitDto>(unit);
            dto.BuildingNumber = buildingDict.GetValueOrDefault(unit.BuildingId);
            return dto;
        }).ToList();

        return result;
    }
}