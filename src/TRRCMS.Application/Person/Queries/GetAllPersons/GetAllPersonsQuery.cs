using MediatR;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetAllPersons;

/// <summary>
/// Query to get all persons with pagination
/// </summary>
public class GetAllPersonsQuery : PagedQuery, IRequest<PagedResult<PersonDto>> { }
