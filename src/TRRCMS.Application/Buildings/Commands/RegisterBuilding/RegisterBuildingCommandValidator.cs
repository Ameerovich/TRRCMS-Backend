using FluentValidation;

namespace TRRCMS.Application.Buildings.Commands.RegisterBuilding;

/// <summary>
/// Validator for RegisterBuildingCommand (QGIS plugin endpoint).
/// Same admin code rules as CreateBuildingCommandValidator.
/// BuildingGeometryWkt is required (QGIS always provides polygon).
/// </summary>
public class RegisterBuildingCommandValidator : AbstractValidator<RegisterBuildingCommand>
{
    public RegisterBuildingCommandValidator()
    {
        RuleFor(x => x.GovernorateCode)
            .NotEmpty().WithMessage("Governorate code is required")
            .Length(2).WithMessage("Governorate code must be exactly 2 digits")
            .Matches(@"^\d{2}$").WithMessage("Governorate code must contain only digits");

        RuleFor(x => x.DistrictCode)
            .NotEmpty().WithMessage("District code is required")
            .Length(2).WithMessage("District code must be exactly 2 digits")
            .Matches(@"^\d{2}$").WithMessage("District code must contain only digits");

        RuleFor(x => x.SubDistrictCode)
            .NotEmpty().WithMessage("Sub-district code is required")
            .Length(2).WithMessage("Sub-district code must be exactly 2 digits")
            .Matches(@"^\d{2}$").WithMessage("Sub-district code must contain only digits");

        RuleFor(x => x.CommunityCode)
            .NotEmpty().WithMessage("Community code is required")
            .Length(3).WithMessage("Community code must be exactly 3 digits")
            .Matches(@"^\d{3}$").WithMessage("Community code must contain only digits");

        RuleFor(x => x.NeighborhoodCode)
            .NotEmpty().WithMessage("Neighborhood code is required")
            .Length(3).WithMessage("Neighborhood code must be exactly 3 digits")
            .Matches(@"^\d{3}$").WithMessage("Neighborhood code must contain only digits");

        RuleFor(x => x.BuildingNumber)
            .NotEmpty().WithMessage("Building number is required")
            .Length(5).WithMessage("Building number must be exactly 5 digits")
            .Matches(@"^\d{5}$").WithMessage("Building number must contain only digits");

        // Composite Building ID validation (17 digits total)
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


        RuleFor(x => x.BuildingGeometryWkt)
            .NotEmpty().WithMessage("Building geometry (WKT polygon) is required for QGIS registration");


        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 2000 characters");
    }
}
