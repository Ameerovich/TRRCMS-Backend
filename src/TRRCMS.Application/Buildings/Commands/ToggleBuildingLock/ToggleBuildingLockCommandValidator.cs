using FluentValidation;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Buildings.Commands.ToggleBuildingLock;

public class ToggleBuildingLockCommandValidator : LocalizedValidator<ToggleBuildingLockCommand>
{
    public ToggleBuildingLockCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty().WithMessage(L("Building_ToggleLockRequired"));
    }
}
