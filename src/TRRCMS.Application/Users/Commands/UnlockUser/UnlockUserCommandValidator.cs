using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Users.Commands.UnlockUser;

/// <summary>
/// Validator for UnlockUserCommand
/// </summary>
public class UnlockUserCommandValidator : LocalizedValidator<UnlockUserCommand>
{
    public UnlockUserCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(L("UserId_Required"));
    }
}
