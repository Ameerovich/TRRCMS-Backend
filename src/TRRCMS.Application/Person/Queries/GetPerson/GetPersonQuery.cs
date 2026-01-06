using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetPerson
{
    public class GetPersonQuery : IRequest<PersonDto?>
    {
        public Guid Id { get; set; }

        public GetPersonQuery(Guid id)
        {
            Id = id;
        }
    }
}