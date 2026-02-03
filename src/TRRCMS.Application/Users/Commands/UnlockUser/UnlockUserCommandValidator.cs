using FluentValidation;

namespace TRRCMS.Application.Users.Commands.UnlockUser;

/// <summary>
/// Validator for UnlockUserCommand
/// </summary>
public class UnlockUserCommandValidator : AbstractValidator<UnlockUserCommand>
{
    public UnlockUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
