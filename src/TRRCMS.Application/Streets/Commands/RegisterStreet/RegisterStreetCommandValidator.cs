using FluentValidation;

namespace TRRCMS.Application.Streets.Commands.RegisterStreet;

public class RegisterStreetCommandValidator : AbstractValidator<RegisterStreetCommand>
{
    public RegisterStreetCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .GreaterThan(0).WithMessage("Identifier must be a positive integer");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Street name is required")
            .MaximumLength(500).WithMessage("Street name cannot exceed 500 characters");

        RuleFor(x => x.GeometryWkt)
            .NotEmpty().WithMessage("Street geometry (WKT linestring) is required for street registration");
    }
}
