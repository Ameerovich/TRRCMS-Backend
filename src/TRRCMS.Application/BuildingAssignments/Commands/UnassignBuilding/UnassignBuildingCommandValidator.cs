using FluentValidation;

namespace TRRCMS.Application.BuildingAssignments.Commands.UnassignBuilding;

/// <summary>
/// Validator for UnassignBuildingCommand
/// </summary>
public class UnassignBuildingCommandValidator : AbstractValidator<UnassignBuildingCommand>
{
    public UnassignBuildingCommandValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty()
            .WithMessage("Assignment ID is required");

        RuleFor(x => x.CancellationReason)
            .NotEmpty()
            .WithMessage("Cancellation reason is required")
            .MaximumLength(1000)
            .WithMessage("Cancellation reason cannot exceed 1000 characters");
    }
}
