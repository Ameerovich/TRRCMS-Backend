using FluentValidation;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuilding;

/// <summary>
/// Validator for UpdateBuildingCommand
/// Enhanced with Syria geographic bounds and upper unit limits
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

        // Unit count validations (with upper limits)
        RuleFor(x => x.NumberOfPropertyUnits)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfPropertyUnits.HasValue)
            .WithMessage("Number of property units cannot be negative");

        RuleFor(x => x.NumberOfPropertyUnits)
            .LessThanOrEqualTo(500)
            .When(x => x.NumberOfPropertyUnits.HasValue)
            .WithMessage("Number of property units cannot exceed 500");

        RuleFor(x => x.NumberOfApartments)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfApartments.HasValue)
            .WithMessage("Number of apartments cannot be negative");

        RuleFor(x => x.NumberOfApartments)
            .LessThanOrEqualTo(500)
            .When(x => x.NumberOfApartments.HasValue)
            .WithMessage("Number of apartments cannot exceed 500");

        RuleFor(x => x.NumberOfShops)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfShops.HasValue)
            .WithMessage("Number of shops cannot be negative");

        RuleFor(x => x.NumberOfShops)
            .LessThanOrEqualTo(200)
            .When(x => x.NumberOfShops.HasValue)
            .WithMessage("Number of shops cannot exceed 200");

        // Syria geographic bounds for coordinates
        // FIX: Use decimal suffix (m) to match decimal? property type
        // Syria approximate bounds: Lat 32.0°N - 37.5°N, Lng 35.5°E - 42.5°E

        RuleFor(x => x.Latitude)
            .InclusiveBetween(32.0m, 37.5m)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between 32.0 and 37.5 (Syria bounds)");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(35.5m, 42.5m)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between 35.5 and 42.5 (Syria bounds)");

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