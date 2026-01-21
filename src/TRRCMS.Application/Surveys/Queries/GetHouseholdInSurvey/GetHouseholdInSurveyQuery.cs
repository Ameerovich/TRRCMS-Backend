using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetHouseholdInSurvey;

/// <summary>
/// Query to get household details in survey context
/// </summary>
public class GetHouseholdInSurveyQuery : IRequest<HouseholdDto>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Household ID to retrieve
    /// </summary>
    public Guid HouseholdId { get; set; }
}