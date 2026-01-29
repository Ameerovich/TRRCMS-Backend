using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetHouseholdsForSurvey;

/// <summary>
/// Query to get all households linked to a survey's property unit
/// </summary>
public class GetHouseholdsForSurveyQuery : IRequest<List<HouseholdDto>>
{
    /// <summary>
    /// Survey ID for authorization and property unit lookup
    /// </summary>
    public Guid SurveyId { get; set; }
}
