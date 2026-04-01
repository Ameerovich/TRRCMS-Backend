using FluentValidation;

namespace TRRCMS.Application.Buildings.Commands.ToggleBuildingLock;

public class ToggleBuildingLockCommandValidator : AbstractValidator<ToggleBuildingLockCommand>
{
    public ToggleBuildingLockCommandValidator()
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty().WithMessage("Building ID is required.");
    }
}
