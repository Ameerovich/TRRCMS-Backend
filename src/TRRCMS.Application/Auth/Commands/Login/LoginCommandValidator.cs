using FluentValidation;

namespace TRRCMS.Application.Auth.Commands.Login;

/// <summary>
/// Validator for LoginCommand
/// Validates login credentials format before authentication attempt
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters");

        RuleFor(x => x.DeviceId)
            .MaximumLength(100).WithMessage("Device ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceId));
    }
}
