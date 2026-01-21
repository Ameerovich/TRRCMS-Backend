using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetHouseholdPersons;

/// <summary>
/// Query to get all persons/members in a household
/// </summary>
public class GetHouseholdPersonsQuery : IRequest<List<PersonDto>>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Household ID to get members for
    /// </summary>
    public Guid HouseholdId { get; set; }
}