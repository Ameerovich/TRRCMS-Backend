using FluentValidation;

namespace TRRCMS.Application.PropertyUnits.Commands.DeletePropertyUnit;

public class DeletePropertyUnitCommandValidator : AbstractValidator<DeletePropertyUnitCommand>
{
    public DeletePropertyUnitCommandValidator()
    {
        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage("PropertyUnit ID is required");
    }
}
