using MediatR;

namespace TRRCMS.Application.Surveys.Commands.DeleteIdentificationDocument;

/// <summary>
/// Command to delete an identification document (soft delete).
/// Also removes the physical file from storage.
/// </summary>
public class DeleteIdentificationDocumentCommand : IRequest<Unit>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Identification document ID to delete
    /// </summary>
    public Guid DocumentId { get; set; }
}
