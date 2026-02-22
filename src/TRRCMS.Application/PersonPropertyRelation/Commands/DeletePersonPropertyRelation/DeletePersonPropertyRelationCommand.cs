using MediatR;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.DeletePersonPropertyRelation;

/// <summary>
/// Command to soft delete a person-property relation
/// Cascades to delete all related evidences
/// Validates survey status is Draft before deletion
/// </summary>
public class DeletePersonPropertyRelationCommand : IRequest<DeleteResultDto>
{
    public Guid RelationId { get; set; }
}
