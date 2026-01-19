using FluentValidation;

namespace TRRCMS.Application.Users.Commands.GrantPermissions;

/// <summary>
/// Validator for GrantPermissionsCommand
/// </summary>
public class GrantPermissionsCommandValidator : AbstractValidator<GrantPermissionsCommand>
{
    public GrantPermissionsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Permissions)
            .NotEmpty().WithMessage("At least one permission must be specified")
            .Must(permissions => permissions != null && permissions.Count > 0)
            .WithMessage("Permission list cannot be empty");

        RuleFor(x => x.GrantReason)
            .NotEmpty().WithMessage("Grant reason is required")
            .MinimumLength(10).WithMessage("Grant reason must be at least 10 characters")
            .MaximumLength(500).WithMessage("Grant reason cannot exceed 500 characters");
    }
}