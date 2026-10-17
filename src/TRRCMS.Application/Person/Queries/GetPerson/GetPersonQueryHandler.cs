using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetPerson;

/// <summary>
/// Handler for GetPersonQuery
/// </summary>
public class GetPersonQueryHandler : IRequestHandler<GetPersonQuery, PersonDto?>
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IPersonDesignationService _designationService;

    public GetPersonQueryHandler(
        IPersonRepository personRepository,
        IMapper mapper,
        IPersonDesignationService designationService)
    {
        _designationService = designationService;
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonDto?> Handle(GetPersonQuery request, CancellationToken cancellationToken)
    {
        var person = await _personRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (person == null)
        {
            return null;
        }

        var dto = _mapper.Map<PersonDto>(person);
        await _designationService.ApplyAsync(new[] { dto }, cancellationToken);
        return dto;
    }
}
