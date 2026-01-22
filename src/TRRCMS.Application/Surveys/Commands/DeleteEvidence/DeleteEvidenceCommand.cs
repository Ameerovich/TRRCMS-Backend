using MediatR;

namespace TRRCMS.Application.Surveys.Commands.DeleteEvidence;

/// <summary>
/// Command to delete evidence (soft delete)
/// Also deletes physical file from storage
/// </summary>
public class DeleteEvidenceCommand : IRequest<Unit>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Evidence ID to delete
    /// </summary>
    public Guid EvidenceId { get; set; }
}