using FluentValidation;

namespace TRRCMS.Application.Streets.Commands.BulkRegisterStreets;

public class BulkRegisterStreetsCommandValidator : AbstractValidator<BulkRegisterStreetsCommand>
{
    public BulkRegisterStreetsCommandValidator()
    {
        RuleFor(x => x.Streets)
            .NotEmpty().WithMessage("At least one street is required.");

        RuleForEach(x => x.Streets).ChildRules(item =>
        {
            item.RuleFor(x => x.Identifier)
                .GreaterThan(0).WithMessage("Identifier must be a positive integer.");

            item.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Street name is required.")
                .MaximumLength(200).WithMessage("Street name cannot exceed 200 characters.");

            item.RuleFor(x => x.GeometryWkt)
                .NotEmpty().WithMessage("Geometry (WKT linestring) is required.");
        });
    }
}
