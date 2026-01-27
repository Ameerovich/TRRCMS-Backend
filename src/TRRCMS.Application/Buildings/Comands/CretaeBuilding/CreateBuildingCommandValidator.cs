using FluentValidation;

namespace TRRCMS.Application.Buildings.Commands.CreateBuilding;

/// <summary>
/// Validator for CreateBuildingCommand
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

        // ==================== BUILDING ATTRIBUTES ====================

        RuleFor(x => x.BuildingType)
            .IsInEnum().WithMessage("Invalid building type (نوع البناء)");

        RuleFor(x => x.BuildingStatus)
            .IsInEnum().WithMessage("Invalid building status (حالة البناء)");

        RuleFor(x => x.NumberOfPropertyUnits)
            .GreaterThanOrEqualTo(0).WithMessage("Number of property units cannot be negative");

        RuleFor(x => x.NumberOfApartments)
            .GreaterThanOrEqualTo(0).WithMessage("Number of apartments cannot be negative");

        RuleFor(x => x.NumberOfShops)
            .GreaterThanOrEqualTo(0).WithMessage("Number of shops cannot be negative");

        // ==================== LOCATION ====================

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180");

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