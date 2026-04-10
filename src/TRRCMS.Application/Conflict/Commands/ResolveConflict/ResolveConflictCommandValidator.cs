using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Commands.ResolveConflict;

/// <summary>
/// Validator for <see cref="ResolveConflictCommand"/>.
/// Enforces: conflict ID required, valid action, mandatory reason.
/// </summary>
public class ResolveConflictCommandValidator : LocalizedValidator<ResolveConflictCommand>
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

    public ResolveConflictCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ConflictId)
            .NotEmpty()
            .WithMessage(L("ConflictId_Required"));

        RuleFor(x => x.Action)
            .Must(a => ValidActions.Contains(a))
            .WithMessage(
                L("ConflictAction_Invalid", string.Join(", ", ValidActions.Select(a => a.ToString()))));

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(L("ResolutionReason_Required"))
            .MaximumLength(2000)
            .WithMessage(L("ResolutionReason_MaxLength2000"));

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .WithMessage(L("ResolutionNotes_MaxLength4000"));
    }
}
