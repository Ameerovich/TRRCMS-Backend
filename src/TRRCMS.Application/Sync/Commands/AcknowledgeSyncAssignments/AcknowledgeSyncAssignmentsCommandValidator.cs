using FluentValidation;

namespace TRRCMS.Application.Sync.Commands.AcknowledgeSyncAssignments;

/// <summary>
/// FluentValidation validator for <see cref="AcknowledgeSyncAssignmentsCommand"/>.
/// Executed automatically by the MediatR <c>ValidationBehavior</c> pipeline before
/// the command handler is invoked.
///
/// Rules enforce the contract between the tablet and the server:
/// <list type="bullet">
///   <item><see cref="AcknowledgeSyncAssignmentsCommand.SyncSessionId"/> must be a
///         non-empty <see cref="Guid"/>.</item>
///   <item><see cref="AcknowledgeSyncAssignmentsCommand.AssignmentIds"/> must contain
///         at least one ID and no more than 500 (prevents unbounded payload).</item>
///   <item>Each individual assignment ID must itself be non-empty.</item>
/// </list>
/// </summary>
public sealed class AcknowledgeSyncAssignmentsCommandValidator
    : AbstractValidator<AcknowledgeSyncAssignmentsCommand>
{
    /// <summary>Maximum number of assignment IDs accepted in a single ack request.</summary>
    private const int MaxAssignmentIds = 500;

    public AcknowledgeSyncAssignmentsCommandValidator()
    {
        // ── Sync session ──────────────────────────────────────────────────────────

        RuleFor(x => x.SyncSessionId)
            .NotEmpty()
            .WithMessage("SyncSessionId is required.");

        // ── Assignment ID list ────────────────────────────────────────────────────

        RuleFor(x => x.AssignmentIds)
            .NotNull()
            .WithMessage("AssignmentIds must not be null.")
            .NotEmpty()
            .WithMessage("At least one assignment ID must be provided.")
            .Must(ids => ids.Count <= MaxAssignmentIds)
            .WithMessage($"A maximum of {MaxAssignmentIds} assignment IDs may be acknowledged per request.");

        // Each individual ID must be non-empty.
        RuleForEach(x => x.AssignmentIds)
            .NotEmpty()
            .WithMessage("Each assignment ID must be a valid non-empty GUID.");
    }
}
