using MediatR;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Buildings.Commands.DeleteBuilding;

/// <summary>
/// Command to soft delete a building
/// Cascades to delete all PropertyUnits and their descendants
/// Validates survey status is Draft before deletion
/// </summary>
public class DeleteBuildingCommand : IRequest<DeleteResultDto>
{
    public Guid BuildingId { get; set; }
}
