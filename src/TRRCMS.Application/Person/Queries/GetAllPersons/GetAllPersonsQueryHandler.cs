using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetAllPersons;

/// <summary>
/// Handler for GetAllPersonsQuery
/// </summary>
public class GetAllPersonsQueryHandler : IRequestHandler<GetAllPersonsQuery, PagedResult<PersonDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;

    public GetAllPersonsQueryHandler(
        IPersonRepository personRepository,
        IMapper mapper)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedResult<PersonDto>> Handle(GetAllPersonsQuery request, CancellationToken cancellationToken)
    {
        var persons = await _personRepository.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<PersonDto>>(persons);
        return PaginatedList.FromEnumerable(dtos, request.PageNumber, request.PageSize);
    }
}
