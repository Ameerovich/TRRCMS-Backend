using FluentValidation;

namespace TRRCMS.Application.Landmarks.Commands.RegisterLandmark;

public class RegisterLandmarkCommandValidator : AbstractValidator<RegisterLandmarkCommand>
{
    public RegisterLandmarkCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .GreaterThan(0).WithMessage("Identifier must be a positive integer");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Landmark name is required")
            .MaximumLength(500).WithMessage("Landmark name cannot exceed 500 characters");

        RuleFor(x => x.Type)
            .InclusiveBetween(1, 10).WithMessage("Landmark type must be between 1 and 10");

        RuleFor(x => x.LocationWkt)
            .NotEmpty().WithMessage("Location geometry (WKT point) is required for landmark registration");
    }
}
