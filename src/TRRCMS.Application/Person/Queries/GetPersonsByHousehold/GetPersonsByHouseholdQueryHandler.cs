using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetPersonsByHousehold;

/// <summary>
/// Handler for GetPersonsByHouseholdQuery
/// </summary>
public class GetPersonsByHouseholdQueryHandler : IRequestHandler<GetPersonsByHouseholdQuery, List<PersonDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IPersonDesignationService _designationService;

    public GetPersonsByHouseholdQueryHandler(
        IPersonRepository personRepository,
        IMapper mapper,
        IPersonDesignationService designationService)
    {
        _designationService = designationService;
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<PersonDto>> Handle(GetPersonsByHouseholdQuery request, CancellationToken cancellationToken)
    {
        var persons = await _personRepository.GetByHouseholdIdAsync(request.HouseholdId, cancellationToken);
        var dtos = _mapper.Map<List<PersonDto>>(persons);
        await _designationService.ApplyAsync(dtos, cancellationToken);
        return dtos;
    }
}
