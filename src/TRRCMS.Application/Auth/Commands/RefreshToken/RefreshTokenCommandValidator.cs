using FluentValidation;

namespace TRRCMS.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Validator for RefreshTokenCommand
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");

        RuleFor(x => x.DeviceId)
            .MaximumLength(100).WithMessage("Device ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceId));
    }
}
