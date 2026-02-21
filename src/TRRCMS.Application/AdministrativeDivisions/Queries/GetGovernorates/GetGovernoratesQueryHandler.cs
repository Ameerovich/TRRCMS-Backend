using AutoMapper;
using MediatR;
using TRRCMS.Application.AdministrativeDivisions.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.AdministrativeDivisions.Queries.GetGovernorates;

/// <summary>
/// Handler for GetGovernoratesQuery
/// </summary>
public class GetGovernoratesQueryHandler : IRequestHandler<GetGovernoratesQuery, List<GovernorateDto>>
{
    private readonly IGovernorateRepository _governorateRepository;
    private readonly IMapper _mapper;

    public GetGovernoratesQueryHandler(
        IGovernorateRepository governorateRepository,
        IMapper mapper)
    {
        _governorateRepository = governorateRepository;
        _mapper = mapper;
    }

    public async Task<List<GovernorateDto>> Handle(
        GetGovernoratesQuery request,
        CancellationToken cancellationToken)
    {
        var governorates = await _governorateRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<GovernorateDto>>(governorates);
    }
}
