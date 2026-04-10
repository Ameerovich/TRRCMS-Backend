using FluentValidation;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Buildings.Commands.DeleteBuilding;

/// <summary>
/// Validator for DeleteBuildingCommand
/// Ensures audit trail compliance for delete operations
/// </summary>
public class DeleteBuildingCommandValidator : LocalizedValidator<DeleteBuildingCommand>
{
    public DeleteBuildingCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage(L("Building_ToggleLockRequired"));
    }
}
