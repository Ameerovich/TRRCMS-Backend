using FluentValidation;

namespace TRRCMS.Application.Buildings.Commands.CreateBuilding;

/// <summary>
/// Validator for CreateBuildingCommand
/// Enhanced with FSD-compliant validations:
/// - Composite BuildingId 17-digit pattern
/// - Syria geographic bounds for coordinates
/// - Upper limits for unit counts
/// </summary>
public class CreateBuildingCommandValidator : AbstractValidator<CreateBuildingCommand>
{
    public CreateBuildingCommandValidator()
    {
        // ==================== ADMINISTRATIVE CODES ====================

        RuleFor(x => x.GovernorateCode)
            .NotEmpty().WithMessage("Governorate code (محافظة) is required")
            .Length(2).WithMessage("Governorate code must be 2 digits")
            .Matches(@"^\d{2}$").WithMessage("Governorate code must contain only digits");

        RuleFor(x => x.DistrictCode)
            .NotEmpty().WithMessage("District code (مدينة) is required")
            .Length(2).WithMessage("District code must be 2 digits")
            .Matches(@"^\d{2}$").WithMessage("District code must contain only digits");

        RuleFor(x => x.SubDistrictCode)
            .NotEmpty().WithMessage("Sub-district code (بلدة) is required")
            .Length(2).WithMessage("Sub-district code must be 2 digits")
            .Matches(@"^\d{2}$").WithMessage("Sub-district code must contain only digits");

        RuleFor(x => x.CommunityCode)
            .NotEmpty().WithMessage("Community code (قرية) is required")
            .Length(3).WithMessage("Community code must be 3 digits")
            .Matches(@"^\d{3}$").WithMessage("Community code must contain only digits");

        RuleFor(x => x.NeighborhoodCode)
            .NotEmpty().WithMessage("Neighborhood code (حي) is required")
            .Length(3).WithMessage("Neighborhood code must be 3 digits")
            .Matches(@"^\d{3}$").WithMessage("Neighborhood code must contain only digits");

        RuleFor(x => x.BuildingNumber)
            .NotEmpty().WithMessage("Building number (رقم البناء) is required")
            .Length(5).WithMessage("Building number must be 5 digits")
            .Matches(@"^\d{5}$").WithMessage("Building number must contain only digits");

        // ==================== COMPOSITE BUILDING ID (17 digits) ====================
        // BuildingId = GovernorateCode(2) + DistrictCode(2) + SubDistrictCode(2)
        //            + CommunityCode(3) + NeighborhoodCode(3) + BuildingNumber(5) = 17 digits
        RuleFor(x => x)
            .Must(x =>
            {
                var compositeId = $"{x.GovernorateCode}{x.DistrictCode}{x.SubDistrictCode}" +
                                  $"{x.CommunityCode}{x.NeighborhoodCode}{x.BuildingNumber}";
                return compositeId.Length == 17 && compositeId.All(char.IsDigit);
            })
            .When(x => !string.IsNullOrEmpty(x.GovernorateCode) &&
                        !string.IsNullOrEmpty(x.DistrictCode) &&
                        !string.IsNullOrEmpty(x.SubDistrictCode) &&
                        !string.IsNullOrEmpty(x.CommunityCode) &&
                        !string.IsNullOrEmpty(x.NeighborhoodCode) &&
                        !string.IsNullOrEmpty(x.BuildingNumber))
            .WithMessage("Composite Building ID must form exactly 17 digits (2+2+2+3+3+5)");

        // ==================== BUILDING ATTRIBUTES ====================

        RuleFor(x => x.BuildingType)
            .IsInEnum().WithMessage("Invalid building type (نوع البناء)");

        RuleFor(x => x.BuildingStatus)
            .IsInEnum().WithMessage("Invalid building status (حالة البناء)");

        RuleFor(x => x.NumberOfPropertyUnits)
            .GreaterThanOrEqualTo(0).WithMessage("Number of property units cannot be negative")
            .LessThanOrEqualTo(500).WithMessage("Number of property units cannot exceed 500");

        RuleFor(x => x.NumberOfApartments)
            .GreaterThanOrEqualTo(0).WithMessage("Number of apartments cannot be negative")
            .LessThanOrEqualTo(500).WithMessage("Number of apartments cannot exceed 500");

        RuleFor(x => x.NumberOfShops)
            .GreaterThanOrEqualTo(0).WithMessage("Number of shops cannot be negative")
            .LessThanOrEqualTo(200).WithMessage("Number of shops cannot exceed 200");

        // Apartments + Shops should not exceed total PropertyUnits
        RuleFor(x => x)
            .Must(x => (x.NumberOfApartments + x.NumberOfShops) <= x.NumberOfPropertyUnits)
            .When(x => x.NumberOfPropertyUnits > 0)
            .WithMessage("Sum of apartments and shops cannot exceed total number of property units");

        // ==================== LOCATION (Syria bounds) ====================
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

        // ==================== DESCRIPTIONS ====================

        RuleFor(x => x.LocationDescription)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.LocationDescription))
            .WithMessage("Location description cannot exceed 1000 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 2000 characters");
    }
}