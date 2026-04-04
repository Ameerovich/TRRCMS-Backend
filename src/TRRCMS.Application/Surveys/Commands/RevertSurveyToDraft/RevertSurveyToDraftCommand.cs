using MediatR;

namespace TRRCMS.Application.Surveys.Commands.RevertSurveyToDraft;

/// <summary>
/// Command to revert a finalized survey back to Draft status.
/// POST /api/v1/surveys/{id}/revert-to-draft
/// Restricted to Admin and DataManager roles.
/// </summary>
public class RevertSurveyToDraftCommand : IRequest<Unit>
{
    /// <summary>
    /// Survey ID to revert (set from route parameter)
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Reason for reverting to draft (required for audit trail)
    /// </summary>
    public string Reason { get; set; } = null!;
}
