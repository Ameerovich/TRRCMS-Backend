using MediatR;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Commands.EscalateConflict;

/// <summary>
/// Command to escalate a conflict to senior/supervisor review.
/// Sets IsEscalated=true and Priority=High on the conflict.
/// </summary>
public class EscalateConflictCommand : IRequest<ConflictDetailDto>
{
    /// <summary>
    /// Conflict resolution ID (set from route parameter).
    /// </summary>
    public Guid ConflictId { get; set; }

    /// <summary>
    /// Mandatory reason for escalation.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
