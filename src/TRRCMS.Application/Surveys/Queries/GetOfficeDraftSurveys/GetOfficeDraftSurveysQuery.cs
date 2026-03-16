using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetOfficeDraftSurveys;

/// <summary>
/// Query to get draft office surveys for the current clerk
/// Gets draft office surveys for the current clerk
/// </summary>
public class GetOfficeDraftSurveysQuery : IRequest<List<SurveyDto>>
{
    // No parameters needed - uses current authenticated user
}
