using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Validator for ChangePasswordCommand.
/// Enforces strong password policy.
/// </summary>
public class ChangePasswordCommandValidator : LocalizedValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(L("UserId_Required"));

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(L("CurrentPassword_Required"));

        // Strong password policy - same as CreateUserCommandValidator
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(L("Password_Required"))
            .MinimumLength(8).WithMessage(L("Password_MinLength8"))
            .Matches(@"[A-Z]").WithMessage(L("Password_RequireUpper"))
            .Matches(@"[a-z]").WithMessage(L("Password_RequireLower"))
            .Matches(@"[0-9]").WithMessage(L("Password_RequireDigit"))
            .Matches(@"[^a-zA-Z0-9]").WithMessage(L("Password_RequireSpecial"));

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(L("PasswordConfirm_Required"))
            .Equal(x => x.NewPassword).WithMessage(L("PasswordConfirm_Mismatch"));

        // New password must be different from current
        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage(L("Password_MustDiffer"));

        RuleFor(x => x.ModifiedByUserId)
            .NotEmpty().WithMessage(L("ModifiedByUserId_Required"));
    }
}
