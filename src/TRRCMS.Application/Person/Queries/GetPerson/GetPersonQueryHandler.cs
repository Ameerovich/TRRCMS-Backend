using MediatR;
using AutoMapper;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetPerson
{
    public class GetPersonQueryHandler : IRequestHandler<GetPersonQuery, PersonDto?>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;

        public GetPersonQueryHandler(IPersonRepository personRepository, IMapper mapper)
        {
            _personRepository = personRepository;
            _mapper = mapper;
        }

        public async Task<PersonDto?> Handle(GetPersonQuery request, CancellationToken cancellationToken)
        {
            var person = await _personRepository.GetByIdAsync(request.Id);

            if (person == null)
                return null;

            return _mapper.Map<PersonDto>(person);
        }
    }
}