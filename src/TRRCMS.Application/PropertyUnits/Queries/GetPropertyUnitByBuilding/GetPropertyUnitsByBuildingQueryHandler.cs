using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnitsByBuilding;

/// <summary>
/// Handler for GetPropertyUnitsByBuildingQuery
/// Retrieves all property units for a specific building
/// </summary>
public class GetPropertyUnitsByBuildingQueryHandler : IRequestHandler<GetPropertyUnitsByBuildingQuery, List<PropertyUnitDto>>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetPropertyUnitsByBuildingQueryHandler(
        IPropertyUnitRepository propertyUnitRepository,
        IBuildingRepository buildingRepository,
        IMapper mapper)
    {
        _propertyUnitRepository = propertyUnitRepository;
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<List<PropertyUnitDto>> Handle(GetPropertyUnitsByBuildingQuery request, CancellationToken cancellationToken)
    {
        // Validate building exists
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId, cancellationToken);
        if (building == null)
        {
            throw new NotFoundException($"Building with ID {request.BuildingId} not found");
        }

        // Get all property units for the building
        var propertyUnits = await _propertyUnitRepository.GetByBuildingIdAsync(request.BuildingId, cancellationToken);

        // Map to DTOs
        var result = propertyUnits.Select(unit =>
        {
            var dto = _mapper.Map<PropertyUnitDto>(unit);
            dto.BuildingNumber = building.BuildingNumber;
            return dto;
        }).ToList();

        return result;
    }
}
