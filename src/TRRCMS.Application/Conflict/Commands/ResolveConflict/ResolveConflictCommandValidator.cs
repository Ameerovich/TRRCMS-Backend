using FluentValidation;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Commands.ResolveConflict;

/// <summary>
/// Validator for <see cref="ResolveConflictCommand"/>.
/// Enforces: conflict ID required, valid action, mandatory reason.
/// </summary>
public class ResolveConflictCommandValidator : AbstractValidator<ResolveConflictCommand>
{
    /// <summary>
    /// Actions that are valid for manual resolution via this command.
    /// Excludes PendingReview (not a resolution), Escalate (separate command),
    /// and Resolved (internal state).
    /// </summary>
    private static readonly ConflictResolutionAction[] ValidActions =
    {
        ConflictResolutionAction.Merge,
        ConflictResolutionAction.KeepBoth,
        ConflictResolutionAction.KeepFirst,
        ConflictResolutionAction.KeepSecond,
        ConflictResolutionAction.Ignored
    };

    public ResolveConflictCommandValidator()
    {
        RuleFor(x => x.ConflictId)
            .NotEmpty()
            .WithMessage("Conflict ID is required.");

        RuleFor(x => x.Action)
            .Must(a => ValidActions.Contains(a))
            .WithMessage(
                $"Action must be one of: {string.Join(", ", ValidActions.Select(a => a.ToString()))}.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Resolution reason is required.")
            .MaximumLength(2000)
            .WithMessage("Resolution reason must not exceed 2000 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .WithMessage("Resolution notes must not exceed 4000 characters.");
    }
}
