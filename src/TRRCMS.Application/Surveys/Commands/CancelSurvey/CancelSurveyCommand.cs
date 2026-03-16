using MediatR;

namespace TRRCMS.Application.Surveys.Commands.CancelSurvey;

/// <summary>
/// Command to cancel a survey (Draft or Finalized → Cancelled).
/// POST /api/v1/surveys/{id}/cancel
/// </summary>
public class CancelSurveyCommand : IRequest<Unit>
{
    /// <summary>
    /// Survey ID to cancel (set from route parameter)
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Reason for cancellation (required for audit trail)
    /// </summary>
    public string Reason { get; set; } = null!;
}
