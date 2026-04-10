using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Users.Commands.ActivateUser;

/// <summary>
/// Validator for ActivateUserCommand
/// </summary>
public class ActivateUserCommandValidator : LocalizedValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(L("UserId_Required"));
    }
}
