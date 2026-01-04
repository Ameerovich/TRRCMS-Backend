using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;

/// <summary>
/// Handler for getting all property units
/// </summary>
public class GetAllPropertyUnitsQueryHandler : IRequestHandler<GetAllPropertyUnitsQuery, List<PropertyUnitDto>>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IMapper _mapper;

    public GetAllPropertyUnitsQueryHandler(
        IPropertyUnitRepository propertyUnitRepository,
        IMapper mapper)
    {
        _propertyUnitRepository = propertyUnitRepository;
        _mapper = mapper;
    }

    public async Task<List<PropertyUnitDto>> Handle(GetAllPropertyUnitsQuery request, CancellationToken cancellationToken)
    {
        var propertyUnits = await _propertyUnitRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<List<PropertyUnitDto>>(propertyUnits);
    }
}