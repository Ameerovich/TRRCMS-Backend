using MediatR;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Commands.ResolveConflict;

/// <summary>
/// Command to resolve a conflict with the chosen resolution action. 
/// When action is Merge, the handler invokes the appropriate IMergeService
/// (PersonMergeService or PropertyMergeService) to perform FK propagation and entity consolidation.
/// </summary>
public class ResolveConflictCommand : IRequest<ConflictDetailDto>
{
    /// <summary>
    /// Conflict resolution ID (set from route parameter).
    /// </summary>
    public Guid ConflictId { get; set; }

    /// <summary>
    /// Resolution action: Merge, KeepBoth, KeepFirst, KeepSecond, Ignored.
    /// </summary>
    public ConflictResolutionAction Action { get; set; }

    /// <summary>
    /// Mandatory reason explaining the resolution decision.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Optional additional notes about the resolution.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// For Merge actions: which entity to keep as master.
    /// If null, the system picks the most complete record.
    /// </summary>
    public Guid? PreferredMasterEntityId { get; set; }
}
