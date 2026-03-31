using FluentValidation;

namespace TRRCMS.Application.Landmarks.Commands.BulkRegisterLandmarks;

public class BulkRegisterLandmarksCommandValidator : AbstractValidator<BulkRegisterLandmarksCommand>
{
    public BulkRegisterLandmarksCommandValidator()
    {
        RuleFor(x => x.Landmarks)
            .NotEmpty().WithMessage("At least one landmark is required.");

        RuleForEach(x => x.Landmarks).ChildRules(item =>
        {
            item.RuleFor(x => x.Identifier)
                .GreaterThan(0).WithMessage("Identifier must be a positive integer.");

            item.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Landmark name is required.")
                .MaximumLength(200).WithMessage("Landmark name cannot exceed 200 characters.");

            item.RuleFor(x => x.Type)
                .InclusiveBetween(1, 10).WithMessage("Landmark type must be between 1 and 10.");

            item.RuleFor(x => x.LocationWkt)
                .NotEmpty().WithMessage("Location geometry (WKT point) is required.");
        });
    }
}
