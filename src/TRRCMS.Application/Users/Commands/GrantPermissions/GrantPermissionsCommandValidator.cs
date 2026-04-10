using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Users.Commands.GrantPermissions;

/// <summary>
/// Validator for GrantPermissionsCommand
/// </summary>
public class GrantPermissionsCommandValidator : LocalizedValidator<GrantPermissionsCommand>
{
    public GrantPermissionsCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(L("UserId_Required"));

        RuleFor(x => x.Permissions)
            .NotEmpty().WithMessage(L("Permission_AtLeastOne"))
            .Must(permissions => permissions != null && permissions.Count > 0)
            .WithMessage(L("Permission_ListNotEmpty"));

        RuleFor(x => x.GrantReason)
            .NotEmpty().WithMessage(L("GrantReason_Required"))
            .MinimumLength(10).WithMessage(L("GrantReason_MinLength10"))
            .MaximumLength(500).WithMessage(L("GrantReason_MaxLength500"));
    }
}
