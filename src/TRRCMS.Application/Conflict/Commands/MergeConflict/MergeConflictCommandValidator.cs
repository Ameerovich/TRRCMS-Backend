using FluentValidation;

namespace TRRCMS.Application.Conflicts.Commands.MergeConflict;

/// <summary>
/// Validator for <see cref="MergeConflictCommand"/>.
/// Enforces: conflict ID required, mandatory justification reason.
/// </summary>
public class MergeConflictCommandValidator : AbstractValidator<MergeConflictCommand>
{
    public MergeConflictCommandValidator()
    {
        RuleFor(x => x.ConflictId)
            .NotEmpty()
            .WithMessage("Conflict ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Justification reason is required for merge decisions.")
            .MaximumLength(2000)
            .WithMessage("Reason must not exceed 2000 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .WithMessage("Notes must not exceed 4000 characters.");
    }
}
