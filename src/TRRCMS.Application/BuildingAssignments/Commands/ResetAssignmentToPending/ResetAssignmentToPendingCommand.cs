using MediatR;

namespace TRRCMS.Application.BuildingAssignments.Commands.ResetAssignmentToPending;

/// <summary>
/// Resets a building assignment's TransferStatus back to Pending so it is
/// picked up on the field collector's next sync, regardless of current status
/// (Transferred, Failed, Cancelled, PartialTransfer, Synchronized, etc.).
/// </summary>
public record ResetAssignmentToPendingCommand : IRequest<ResetAssignmentToPendingResult>
{
    /// <summary>Assignment to reset.</summary>
    public Guid AssignmentId { get; init; }

    /// <summary>Optional reason recorded in AssignmentNotes for audit trail.</summary>
    public string? Reason { get; init; }
}

public class ResetAssignmentToPendingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid AssignmentId { get; set; }
    public string BuildingCode { get; set; } = string.Empty;
    public string FieldCollectorName { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
}
