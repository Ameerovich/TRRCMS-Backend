using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnit;

/// <summary>
/// Handler for GetPropertyUnitQuery
/// </summary>
public class GetPropertyUnitQueryHandler : IRequestHandler<GetPropertyUnitQuery, PropertyUnitDto?>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetPropertyUnitQueryHandler(
        IPropertyUnitRepository propertyUnitRepository,
        IBuildingRepository buildingRepository,
        IMapper mapper)
    {
        _propertyUnitRepository = propertyUnitRepository;
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<PropertyUnitDto?> Handle(GetPropertyUnitQuery request, CancellationToken cancellationToken)
    {
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.Id, cancellationToken);

        if (propertyUnit == null)
        {
            return null;
        }

        // Get building for DTO enrichment
        var building = await _buildingRepository.GetByIdAsync(propertyUnit.BuildingId, cancellationToken);

        // Map to DTO
        var result = _mapper.Map<PropertyUnitDto>(propertyUnit);
        result.BuildingNumber = building?.BuildingNumber;

        return result;
    }
}