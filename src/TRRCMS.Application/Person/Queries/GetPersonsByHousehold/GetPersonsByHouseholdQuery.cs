using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetPersonsByHousehold;

/// <summary>
/// Query to get all persons in a household
/// </summary>
public record GetPersonsByHouseholdQuery(Guid HouseholdId) : IRequest<List<PersonDto>>;
