using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Queries.GetAllHouseholds;

/// <summary>
/// Handler for GetAllHouseholdsQuery
/// </summary>
public class GetAllHouseholdsQueryHandler : IRequestHandler<GetAllHouseholdsQuery, PagedResult<HouseholdDto>>
{
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IMapper _mapper;

    public GetAllHouseholdsQueryHandler(
        IHouseholdRepository householdRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IMapper mapper)
    {
        _householdRepository = householdRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<HouseholdDto>> Handle(GetAllHouseholdsQuery request, CancellationToken cancellationToken)
    {
        var households = await _householdRepository.GetAllAsync(cancellationToken);

        // Batch load all referenced property units (avoids N+1 queries)
        var propertyUnitIds = households.Select(h => h.PropertyUnitId).Distinct().ToList();
        var allPropertyUnits = await _propertyUnitRepository.GetAllAsync(cancellationToken);
        var propertyUnitDict = allPropertyUnits
            .Where(pu => propertyUnitIds.Contains(pu.Id))
            .ToDictionary(pu => pu.Id, pu => pu.UnitIdentifier);

        // Map to DTOs and paginate
        var dtos = households.Select(household =>
        {
            var dto = _mapper.Map<HouseholdDto>(household);
            dto.PropertyUnitIdentifier = propertyUnitDict.GetValueOrDefault(household.PropertyUnitId);
            return dto;
        }).ToList();

        return PaginatedList.FromEnumerable(dtos, request.PageNumber, request.PageSize);
    }
}
