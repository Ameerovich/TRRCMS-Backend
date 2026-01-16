using FluentValidation;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuilding;

public class UpdateBuildingCommandValidator : AbstractValidator<UpdateBuildingCommand>
{
    public UpdateBuildingCommandValidator()
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage("Building ID is required");

        RuleFor(x => x.ReasonForModification)
            .NotEmpty()
            .WithMessage("Reason for modification is required")
            .MinimumLength(10)
            .WithMessage("Reason must be at least 10 characters")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters");

        // Unit count validations
        RuleFor(x => x.NumberOfApartments)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfApartments.HasValue)
            .WithMessage("Number of apartments cannot be negative");

        RuleFor(x => x.NumberOfShops)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfShops.HasValue)
            .WithMessage("Number of shops cannot be negative");

        RuleFor(x => x.NumberOfFloors)
            .GreaterThan(0)
            .When(x => x.NumberOfFloors.HasValue)
            .WithMessage("Number of floors must be positive");

        RuleFor(x => x.YearOfConstruction)
            .GreaterThan(1800)
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 10)
            .When(x => x.YearOfConstruction.HasValue)
            .WithMessage("Year of construction must be between 1800 and 10 years in the future");

        // Text field length validations
        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Address))
            .WithMessage("Address must not exceed 500 characters");

        RuleFor(x => x.Landmark)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Landmark))
            .WithMessage("Landmark must not exceed 200 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes must not exceed 1000 characters");

        // Coordinate validations
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180");

        // Both coordinates must be provided together
        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) ||
                       (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage("Both latitude and longitude must be provided together");
    }
}