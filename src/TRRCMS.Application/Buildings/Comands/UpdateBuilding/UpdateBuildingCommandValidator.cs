using FluentValidation;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuilding;

/// <summary>
/// Validator for UpdateBuildingCommand
/// </summary>
public class UpdateBuildingCommandValidator : AbstractValidator<UpdateBuildingCommand>
{
    public UpdateBuildingCommandValidator()
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage("Building ID is required");

        // Building type validation
        RuleFor(x => x.BuildingType)
            .IsInEnum()
            .When(x => x.BuildingType.HasValue)
            .WithMessage("Invalid building type");

        // Building status validation
        RuleFor(x => x.BuildingStatus)
            .IsInEnum()
            .When(x => x.BuildingStatus.HasValue)
            .WithMessage("Invalid building status");

        // Unit count validations
        RuleFor(x => x.NumberOfPropertyUnits)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfPropertyUnits.HasValue)
            .WithMessage("Number of property units cannot be negative");

        RuleFor(x => x.NumberOfApartments)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfApartments.HasValue)
            .WithMessage("Number of apartments cannot be negative");

        RuleFor(x => x.NumberOfShops)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfShops.HasValue)
            .WithMessage("Number of shops cannot be negative");

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

        // Description validations
        RuleFor(x => x.LocationDescription)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.LocationDescription))
            .WithMessage("Location description cannot exceed 1000 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 2000 characters");
    }
}