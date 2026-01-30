using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetPerson;

/// <summary>
/// Query to get a person by ID
/// </summary>
public record GetPersonQuery(Guid Id) : IRequest<PersonDto?>;
