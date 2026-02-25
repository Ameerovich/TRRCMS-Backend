using MediatR;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Commands.MergeConflict;

/// <summary>
/// Command to merge a duplicate conflict by selecting a master record.
///
/// Delegates to PersonMergeService or PropertyMergeService based on EntityType,
/// then updates the ConflictResolution entity and propagates FK changes.
/// </summary>
public class MergeConflictCommand : IRequest<ConflictDetailDto>
{
    /// <summary>
    /// ConflictResolution surrogate ID (set from route parameter).
    /// </summary>
    public Guid ConflictId { get; set; }

    /// <summary>
    /// ID of the entity to keep as the master (surviving) record.
    /// Must be one of FirstEntityId or SecondEntityId from the conflict.
    /// If null, the system defaults to FirstEntityId.
    /// </summary>
    public Guid? MasterEntityId { get; set; }

    /// <summary>
    /// Mandatory justification notes explaining why merge was chosen.
    /// Required by UC-007/UC-008 audit trail requirements.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Optional additional notes about the resolution.
    /// </summary>
    public string? Notes { get; set; }
}
