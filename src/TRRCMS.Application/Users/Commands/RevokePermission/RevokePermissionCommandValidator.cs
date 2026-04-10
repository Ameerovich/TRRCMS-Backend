using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Users.Commands.RevokePermission;

/// <summary>
/// Validator for RevokePermissionCommand
/// </summary>
public class RevokePermissionCommandValidator : LocalizedValidator<RevokePermissionCommand>
{
    public RevokePermissionCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(L("UserId_Required"));

        RuleFor(x => x.Permission)
            .IsInEnum().WithMessage(L("Permission_Invalid"));

        RuleFor(x => x.RevokeReason)
            .NotEmpty().WithMessage(L("RevokeReason_Required"))
            .MinimumLength(10).WithMessage(L("RevokeReason_MinLength10"))
            .MaximumLength(500).WithMessage(L("RevokeReason_MaxLength500"));
    }
}
