using FluentValidation;

namespace TRRCMS.Application.Conflicts.Commands.EscalateConflict;

/// <summary>
/// Validator for <see cref="EscalateConflictCommand"/>.
/// </summary>
public class EscalateConflictCommandValidator : AbstractValidator<EscalateConflictCommand>
{
    public EscalateConflictCommandValidator()
    {
        RuleFor(x => x.ConflictId)
            .NotEmpty()
            .WithMessage("Conflict ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Escalation reason is required.")
            .MaximumLength(2000)
            .WithMessage("Escalation reason must not exceed 2000 characters.");
    }
}
