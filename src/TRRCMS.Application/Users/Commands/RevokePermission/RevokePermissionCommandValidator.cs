using FluentValidation;

namespace TRRCMS.Application.Users.Commands.RevokePermission;

/// <summary>
/// Validator for RevokePermissionCommand
/// </summary>
public class RevokePermissionCommandValidator : AbstractValidator<RevokePermissionCommand>
{
    public RevokePermissionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Permission)
            .IsInEnum().WithMessage("Invalid permission value");

        RuleFor(x => x.RevokeReason)
            .NotEmpty().WithMessage("Revoke reason is required")
            .MinimumLength(10).WithMessage("Revoke reason must be at least 10 characters")
            .MaximumLength(500).WithMessage("Revoke reason cannot exceed 500 characters");
    }
}