using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetAllPersons
{
    public class GetAllPersonsQuery : IRequest<IEnumerable<PersonDto>>
    {
    }
}