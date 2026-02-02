using MediatR;

namespace TRRCMS.Application.BuildingAssignments.Commands.UnassignBuilding;

/// <summary>
/// Command to unassign/cancel a building assignment
/// UC-012: Supports unassignment workflow
/// </summary>
public record UnassignBuildingCommand : IRequest<UnassignBuildingResult>
{
    /// <summary>
    /// Assignment ID to cancel
    /// </summary>
    public Guid AssignmentId { get; init; }
    
    /// <summary>
    /// Reason for cancellation (required)
    /// </summary>
    public string CancellationReason { get; init; } = string.Empty;
}

/// <summary>
/// Result of the unassign building command
/// </summary>
public class UnassignBuildingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid AssignmentId { get; set; }
    public string BuildingCode { get; set; } = string.Empty;
    public string FieldCollectorName { get; set; } = string.Empty;
}
