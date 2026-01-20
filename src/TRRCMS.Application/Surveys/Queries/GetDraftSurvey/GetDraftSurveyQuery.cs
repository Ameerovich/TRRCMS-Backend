using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetDraftSurvey;

/// <summary>
/// Query to retrieve a draft survey for resuming work
/// Corresponds to UC-002: Resume draft survey
/// </summary>
public class GetDraftSurveyQuery : IRequest<SurveyDto>
{
    /// <summary>
    /// Survey ID to retrieve
    /// </summary>
    public Guid SurveyId { get; set; }
}