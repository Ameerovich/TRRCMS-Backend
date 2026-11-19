using MediatR;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Queries.GetAllPersons;

/// <summary>
/// Query to get all persons with pagination
/// </summary>
public class GetAllPersonsQuery : PagedQuery, IRequest<PagedResult<PersonDto>>
{
    /// <summary>
    /// Optional free-text filter (case-insensitive, partial): first/father/family/mother name,
    /// full name, national ID, mobile/phone number, or the person ID. Empty returns everyone.
    /// </summary>
    public string? Search { get; init; }
}
