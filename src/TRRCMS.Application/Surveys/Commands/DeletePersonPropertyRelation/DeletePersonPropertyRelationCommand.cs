using MediatR;

namespace TRRCMS.Application.Surveys.Commands.DeletePersonPropertyRelation;

/// <summary>
/// Command to delete a person-property relation
/// Also deletes associated evidence documents
/// </summary>
public class DeletePersonPropertyRelationCommand : IRequest<Unit>
{
    public Guid SurveyId { get; set; }
    public Guid RelationId { get; set; }

    /// <summary>
    /// If true (default), also delete evidence files from storage
    /// </summary>
    public bool DeleteEvidenceFiles { get; set; } = true;
}
