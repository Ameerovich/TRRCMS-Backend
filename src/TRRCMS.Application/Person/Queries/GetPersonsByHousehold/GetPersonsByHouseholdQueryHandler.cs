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

    public GetPersonsByHouseholdQueryHandler(
        IPersonRepository personRepository,
        IMapper mapper)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<PersonDto>> Handle(GetPersonsByHouseholdQuery request, CancellationToken cancellationToken)
    {
        var persons = await _personRepository.GetByHouseholdIdAsync(request.HouseholdId, cancellationToken);
        return _mapper.Map<List<PersonDto>>(persons);
    }
}
