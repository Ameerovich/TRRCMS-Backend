using FluentValidation;

namespace TRRCMS.Application.Landmarks.Commands.UpdateLandmark;

public class UpdateLandmarkCommandValidator : AbstractValidator<UpdateLandmarkCommand>
{
    public UpdateLandmarkCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Landmark ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Landmark name is required")
            .MaximumLength(500).WithMessage("Landmark name cannot exceed 500 characters");

        RuleFor(x => x.Type)
            .InclusiveBetween(1, 10).WithMessage("Landmark type must be between 1 and 10");
    }
}
