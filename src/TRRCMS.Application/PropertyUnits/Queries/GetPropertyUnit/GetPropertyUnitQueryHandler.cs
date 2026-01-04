using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnit;

/// <summary>
/// Handler for getting a property unit by ID
/// </summary>
public class GetPropertyUnitQueryHandler : IRequestHandler<GetPropertyUnitQuery, PropertyUnitDto?>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IMapper _mapper;

    public GetPropertyUnitQueryHandler(
        IPropertyUnitRepository propertyUnitRepository,
        IMapper mapper)
    {
        _propertyUnitRepository = propertyUnitRepository;
        _mapper = mapper;
    }

    public async Task<PropertyUnitDto?> Handle(GetPropertyUnitQuery request, CancellationToken cancellationToken)
    {
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.Id, cancellationToken);

        if (propertyUnit == null)
        {
            return null;
        }

        return _mapper.Map<PropertyUnitDto>(propertyUnit);
    }
}