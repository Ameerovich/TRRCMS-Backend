using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Users.Commands.DeactivateUser;

/// <summary>
/// Validator for DeactivateUserCommand
/// </summary>
public class DeactivateUserCommandValidator : LocalizedValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(L("UserId_Required"));

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(L("Deactivation_ReasonRequired"))
            .MinimumLength(10).WithMessage(L("Deactivation_ReasonMinLength10"))
            .MaximumLength(500).WithMessage(L("Deactivation_ReasonMaxLength500"));
    }
}
