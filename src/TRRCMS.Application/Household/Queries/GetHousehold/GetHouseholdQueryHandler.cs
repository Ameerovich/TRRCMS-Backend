using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Queries.GetHousehold;

/// <summary>
/// Handler for GetHouseholdQuery
/// </summary>
public class GetHouseholdQueryHandler : IRequestHandler<GetHouseholdQuery, HouseholdDto?>
{
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IMapper _mapper;

    public GetHouseholdQueryHandler(
        IHouseholdRepository householdRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IMapper mapper)
    {
        _householdRepository = householdRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _mapper = mapper;
    }

    public async Task<HouseholdDto?> Handle(GetHouseholdQuery request, CancellationToken cancellationToken)
    {
        var household = await _householdRepository.GetByIdAsync(request.Id, cancellationToken);

        if (household == null)
        {
            return null;
        }

        // Get property unit for DTO enrichment
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);

        // Map to DTO
        var result = _mapper.Map<HouseholdDto>(household);
        result.PropertyUnitIdentifier = propertyUnit?.UnitIdentifier;

        return result;
    }
}
