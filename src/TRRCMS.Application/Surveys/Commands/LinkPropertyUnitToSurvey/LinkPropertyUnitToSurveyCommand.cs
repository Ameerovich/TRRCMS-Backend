using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.LinkPropertyUnitToSurvey;

/// <summary>
/// Command to link an existing property unit to a survey
/// Corresponds to UC-001 Stage 2: Property Unit Selection - Select Existing Unit
/// </summary>
public class LinkPropertyUnitToSurveyCommand : IRequest<SurveyDto>
{
    /// <summary>
    /// Survey ID to link the property unit to
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Property unit ID to link to the survey
    /// </summary>
    public Guid PropertyUnitId { get; set; }
}