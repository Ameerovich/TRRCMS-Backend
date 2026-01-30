using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetAllPersons;

/// <summary>
/// Query to get all persons
/// </summary>
public record GetAllPersonsQuery : IRequest<List<PersonDto>>;
