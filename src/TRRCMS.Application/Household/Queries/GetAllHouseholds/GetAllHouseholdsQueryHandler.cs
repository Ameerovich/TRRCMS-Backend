using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Queries.GetAllHouseholds;

/// <summary>
/// Handler for GetAllHouseholdsQuery
/// </summary>
public class GetAllHouseholdsQueryHandler : IRequestHandler<GetAllHouseholdsQuery, IEnumerable<HouseholdDto>>
{
    private readonly IHouseholdRepository _householdRepository;
    private readonly IMapper _mapper;

    public GetAllHouseholdsQueryHandler(
        IHouseholdRepository householdRepository,
        IMapper mapper)
    {
        _householdRepository = householdRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<HouseholdDto>> Handle(GetAllHouseholdsQuery request, CancellationToken cancellationToken)
    {
        var households = await _householdRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<HouseholdDto>>(households);
    }
}
