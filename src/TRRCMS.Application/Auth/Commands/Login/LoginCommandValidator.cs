using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Auth.Commands.Login;

/// <summary>
/// Validator for LoginCommand
/// Validates login credentials format before authentication attempt
/// </summary>
public class LoginCommandValidator : LocalizedValidator<LoginCommand>
{
    public LoginCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(L("Username_Required"))
            .MaximumLength(50).WithMessage(L("Username_MaxLength50"));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(L("Password_Required"))
            .MaximumLength(128).WithMessage(L("Password_MaxLength128"));

        RuleFor(x => x.DeviceId)
            .MaximumLength(100).WithMessage(L("DeviceId_MaxLength100"))
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceId));
    }
}
