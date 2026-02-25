using FluentValidation;

namespace TRRCMS.Application.Conflicts.Commands.KeepSeparateConflict;

/// <summary>
/// Validator for <see cref="KeepSeparateConflictCommand"/>.
/// Enforces: conflict ID required, mandatory justification reason.
/// </summary>
public class KeepSeparateConflictCommandValidator : AbstractValidator<KeepSeparateConflictCommand>
{
    public KeepSeparateConflictCommandValidator()
    {
        RuleFor(x => x.ConflictId)
            .NotEmpty()
            .WithMessage("Conflict ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Justification reason is required for keep-separate decisions.")
            .MaximumLength(2000)
            .WithMessage("Reason must not exceed 2000 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .WithMessage("Notes must not exceed 4000 characters.");
    }
}
