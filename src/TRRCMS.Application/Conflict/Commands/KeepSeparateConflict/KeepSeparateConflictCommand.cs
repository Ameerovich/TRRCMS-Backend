using MediatR;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Commands.KeepSeparateConflict;

/// <summary>
/// Command to mark a conflict as reviewed and records as not duplicates (keep separate).
///
/// Sets ConflictResolutionAction.KeepBoth and prevents the same group from being
/// re-surfaced as a duplicate unless detection rules or keys change.
/// </summary>
public class KeepSeparateConflictCommand : IRequest<ConflictDetailDto>
{
    /// <summary>
    /// ConflictResolution surrogate ID (set from route parameter).
    /// </summary>
    public Guid ConflictId { get; set; }

    /// <summary>
    /// Mandatory justification notes explaining why records are not duplicates.
    /// Required by UC-007/UC-008 audit trail requirements.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Optional additional notes about the decision.
    /// </summary>
    public string? Notes { get; set; }
}
