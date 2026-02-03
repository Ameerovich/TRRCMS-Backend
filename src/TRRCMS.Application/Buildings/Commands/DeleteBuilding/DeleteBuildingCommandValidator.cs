using FluentValidation;

namespace TRRCMS.Application.Buildings.Commands.DeleteBuilding;

/// <summary>
/// Validator for DeleteBuildingCommand
/// Ensures audit trail compliance for delete operations
/// </summary>
public class DeleteBuildingCommandValidator : AbstractValidator<DeleteBuildingCommand>
{
    public DeleteBuildingCommandValidator()
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty().WithMessage("Building ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Deletion reason is required for audit trail")
            .MinimumLength(10).WithMessage("Deletion reason must be at least 10 characters")
            .MaximumLength(500).WithMessage("Deletion reason cannot exceed 500 characters");
    }
}
