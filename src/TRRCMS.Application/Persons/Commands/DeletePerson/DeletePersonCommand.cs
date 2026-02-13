using MediatR;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Persons.Commands.DeletePerson;

/// <summary>
/// Command to soft delete a person
/// Cascades to delete all PersonPropertyRelations and their Evidences
/// Validates survey status is Draft before deletion
/// </summary>
public class DeletePersonCommand : IRequest<DeleteResultDto>
{
    public Guid PersonId { get; set; }
}
