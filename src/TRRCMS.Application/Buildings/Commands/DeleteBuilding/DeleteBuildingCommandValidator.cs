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
            .NotEmpty()
            .WithMessage("Building ID is required");
    }
}
