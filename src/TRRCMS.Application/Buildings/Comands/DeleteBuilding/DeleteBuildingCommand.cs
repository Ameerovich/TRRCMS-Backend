using MediatR;

namespace TRRCMS.Application.Buildings.Commands.DeleteBuilding;

/// <summary>
/// Soft delete a building command
/// UC-007: Building Management - Delete Building
/// </summary>
public class DeleteBuildingCommand : IRequest<bool>
{
    /// <summary>
    /// Building ID to delete (GUID)
    /// </summary>
    public Guid BuildingId { get; set; }

    /// <summary>
    /// Reason for deletion (required for audit trail)
    /// سبب الحذف
    /// </summary>
    public string? Reason { get; set; }
}
