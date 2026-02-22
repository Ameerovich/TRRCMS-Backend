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

        // Get all property units for DTO enrichment
        var propertyUnitIds = households.Select(h => h.PropertyUnitId).Distinct();
        var propertyUnitDict = new Dictionary<Guid, string>();

        foreach (var propertyUnitId in propertyUnitIds)
        {
            var propertyUnit = await _propertyUnitRepository.GetByIdAsync(propertyUnitId, cancellationToken);
            if (propertyUnit != null)
            {
                propertyUnitDict[propertyUnitId] = propertyUnit.UnitIdentifier;
            }
        }

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
