using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Validator for RefreshTokenCommand
/// </summary>
public class RefreshTokenCommandValidator : LocalizedValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(L("RefreshToken_Required"));

        RuleFor(x => x.DeviceId)
            .MaximumLength(100).WithMessage(L("DeviceId_MaxLength100"))
            .When(x => !string.IsNullOrWhiteSpace(x.DeviceId));
    }
}
