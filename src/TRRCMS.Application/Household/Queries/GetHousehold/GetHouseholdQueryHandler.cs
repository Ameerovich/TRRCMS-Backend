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
    private readonly IMapper _mapper;

    public GetHouseholdQueryHandler(
        IHouseholdRepository householdRepository,
        IMapper mapper)
    {
        _householdRepository = householdRepository;
        _mapper = mapper;
    }

    public async Task<HouseholdDto?> Handle(GetHouseholdQuery request, CancellationToken cancellationToken)
    {
        var household = await _householdRepository.GetByIdAsync(request.Id, cancellationToken);

        if (household == null)
            return null;

        return _mapper.Map<HouseholdDto>(household);
    }
}
