using FluentValidation;

namespace TRRCMS.Application.Users.Commands.DeactivateUser;

/// <summary>
/// Validator for DeactivateUserCommand
/// </summary>
public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Deactivation reason is required")
            .MinimumLength(10).WithMessage("Deactivation reason must be at least 10 characters")
            .MaximumLength(500).WithMessage("Deactivation reason cannot exceed 500 characters");
    }
}