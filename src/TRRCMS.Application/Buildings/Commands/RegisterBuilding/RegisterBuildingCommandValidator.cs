using FluentValidation;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Buildings.Commands.RegisterBuilding;

/// <summary>
/// Validator for RegisterBuildingCommand (QGIS plugin endpoint).
/// Same admin code rules as CreateBuildingCommandValidator.
/// BuildingGeometryWkt is required (QGIS always provides polygon).
/// </summary>
public class RegisterBuildingCommandValidator : LocalizedValidator<RegisterBuildingCommand>
{
    public RegisterBuildingCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.GovernorateCode)
            .NotEmpty().WithMessage(L("Governorate_Required"))
            .Length(2).WithMessage(L("Governorate_Exactly2Digits"))
            .Matches(@"^\d{2}$").WithMessage(L("Governorate_DigitsOnly"));

        RuleFor(x => x.DistrictCode)
            .NotEmpty().WithMessage(L("District_Required"))
            .Length(2).WithMessage(L("District_Exactly2Digits"))
            .Matches(@"^\d{2}$").WithMessage(L("District_DigitsOnly"));

        RuleFor(x => x.SubDistrictCode)
            .NotEmpty().WithMessage(L("SubDistrict_Required"))
            .Length(2).WithMessage(L("SubDistrict_Exactly2Digits"))
            .Matches(@"^\d{2}$").WithMessage(L("SubDistrict_DigitsOnly"));

        RuleFor(x => x.CommunityCode)
            .NotEmpty().WithMessage(L("Community_Required"))
            .Length(3).WithMessage(L("Community_Exactly3Digits"))
            .Matches(@"^\d{3}$").WithMessage(L("Community_DigitsOnly"));

        RuleFor(x => x.NeighborhoodCode)
            .NotEmpty().WithMessage(L("Neighborhood_Required"))
            .Length(3).WithMessage(L("Neighborhood_Exactly3Digits"))
            .Matches(@"^\d{3}$").WithMessage(L("Neighborhood_DigitsOnly"));

        RuleFor(x => x.BuildingNumber)
            .NotEmpty().WithMessage(L("BuildingNumber_Required"))
            .Length(5).WithMessage(L("BuildingNumber_5Digits"))
            .Matches(@"^\d{5}$").WithMessage(L("BuildingNumber_DigitsOnly"));

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
            .WithMessage(L("BuildingId_CompositeFormat"));


        RuleFor(x => x.BuildingGeometryWkt)
            .NotEmpty().WithMessage(L("Building_GeometryRequired"));


        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage(L("Notes_MaxLength2000"));
    }
}
