using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.LinkPropertyUnitToSurvey;

/// <summary>
/// Command to link an existing property unit to a survey
/// Links an existing property unit to a survey
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