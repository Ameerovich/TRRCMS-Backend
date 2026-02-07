using MediatR;

namespace TRRCMS.Application.Surveys.Commands.FinalizeOfficeSurvey;

/// <summary>
/// Command to mark an office survey as Finalized.
/// Only changes the survey status from Draft → Finalized.
/// Does NOT create claims — use ProcessOfficeSurveyClaimsCommand for that.
/// 
/// POST /api/v1/surveys/office/{id}/finalize
/// </summary>
public class FinalizeOfficeSurveyCommand : IRequest<Unit>
{
    /// <summary>
    /// Survey ID to finalize (set from route parameter)
    /// </summary>
    public Guid SurveyId { get; set; }
}
