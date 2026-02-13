using MediatR;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.PropertyUnits.Commands.DeletePropertyUnit;

/// <summary>
/// Command to soft delete a property unit
/// Cascades to delete:
/// - All Households in this unit
/// - All Persons in those households
/// - All PersonPropertyRelations for those persons and this unit
/// - All Evidences linked to those relations
/// Validates survey status is Draft before deletion
/// </summary>
public class DeletePropertyUnitCommand : IRequest<DeleteResultDto>
{
    public Guid PropertyUnitId { get; set; }
}
