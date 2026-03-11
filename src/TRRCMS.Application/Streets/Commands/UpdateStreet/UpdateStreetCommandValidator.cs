using FluentValidation;

namespace TRRCMS.Application.Streets.Commands.UpdateStreet;

public class UpdateStreetCommandValidator : AbstractValidator<UpdateStreetCommand>
{
    public UpdateStreetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Street ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Street name is required")
            .MaximumLength(500).WithMessage("Street name cannot exceed 500 characters");
    }
}
