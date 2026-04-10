using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.BuildingAssignments.Commands.UnassignBuilding;

/// <summary>
/// Validator for UnassignBuildingCommand
/// </summary>
public class UnassignBuildingCommandValidator : LocalizedValidator<UnassignBuildingCommand>
{
    public UnassignBuildingCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty()
            .WithMessage(L("AssignmentId_Required"));

        RuleFor(x => x.CancellationReason)
            .NotEmpty()
            .WithMessage(L("CancellationReason_Assignment_Required"))
            .MaximumLength(1000)
            .WithMessage(L("CancellationReason_Assignment_MaxLength1000"));
    }
}
