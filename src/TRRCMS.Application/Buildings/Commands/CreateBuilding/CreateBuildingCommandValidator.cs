using FluentValidation;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Buildings.Commands.CreateBuilding;

/// <summary>
/// Validator for CreateBuildingCommand.
/// </summary>
public class CreateBuildingCommandValidator : LocalizedValidator<CreateBuildingCommand>
{
    public CreateBuildingCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        // Raw-code format checks — only fire when the raw value is provided.
        RuleFor(x => x.GovernorateCode!).Length(2).Matches(@"^\d{2}$")
            .When(x => !string.IsNullOrEmpty(x.GovernorateCode))
            .WithMessage(L("Governorate_2Digits"));
        RuleFor(x => x.DistrictCode!).Length(2).Matches(@"^\d{2}$")
            .When(x => !string.IsNullOrEmpty(x.DistrictCode))
            .WithMessage(L("District_2Digits"));
        RuleFor(x => x.SubDistrictCode!).Length(2).Matches(@"^\d{2}$")
            .When(x => !string.IsNullOrEmpty(x.SubDistrictCode))
            .WithMessage(L("SubDistrict_2Digits"));
        RuleFor(x => x.CommunityCode!).Length(3).Matches(@"^\d{3}$")
            .When(x => !string.IsNullOrEmpty(x.CommunityCode))
            .WithMessage(L("Community_3Digits"));
        RuleFor(x => x.NeighborhoodCode!).Length(3).Matches(@"^\d{3}$")
            .When(x => !string.IsNullOrEmpty(x.NeighborhoodCode))
            .WithMessage(L("Neighborhood_3Digits"));

        // OCHA pCode format checks — only fire when a pCode is provided.
        RuleFor(x => x.GovernoratePCode!).Matches(@"^(?i)SY\d{2}$")
            .When(x => !string.IsNullOrEmpty(x.GovernoratePCode))
            .WithMessage("GovernoratePCode must look like 'SY02'.");
        RuleFor(x => x.DistrictPCode!).Matches(@"^(?i)SY\d{4}$")
            .When(x => !string.IsNullOrEmpty(x.DistrictPCode))
            .WithMessage("DistrictPCode must look like 'SY0200'.");
        RuleFor(x => x.SubDistrictPCode!).Matches(@"^(?i)SY\d{6}$")
            .When(x => !string.IsNullOrEmpty(x.SubDistrictPCode))
            .WithMessage("SubDistrictPCode must look like 'SY020000'.");
        RuleFor(x => x.CommunityPCode!).Matches(@"^(?i)C\d{1,9}$")
            .When(x => !string.IsNullOrEmpty(x.CommunityPCode))
            .WithMessage("CommunityPCode must look like 'C1007'.");
        RuleFor(x => x.NeighborhoodPCode!).Matches(@"^(?i)N\d{1,9}$")
            .When(x => !string.IsNullOrEmpty(x.NeighborhoodPCode))
            .WithMessage("NeighborhoodPCode must look like 'N0160'.");

        // Each level must be supplied as either raw OR pCode.
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.GovernorateCode) ||
                !string.IsNullOrEmpty(x.GovernoratePCode) ||
                !string.IsNullOrEmpty(x.DistrictPCode) ||
                !string.IsNullOrEmpty(x.SubDistrictPCode))
            .WithMessage(L("Governorate_Required"));
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.DistrictCode) ||
                !string.IsNullOrEmpty(x.DistrictPCode) ||
                !string.IsNullOrEmpty(x.SubDistrictPCode))
            .WithMessage(L("District_Required"));
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.SubDistrictCode) ||
                !string.IsNullOrEmpty(x.SubDistrictPCode))
            .WithMessage(L("SubDistrict_Required"));
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.CommunityCode) ||
                !string.IsNullOrEmpty(x.CommunityPCode))
            .WithMessage(L("Community_Required"));
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.NeighborhoodCode) ||
                !string.IsNullOrEmpty(x.NeighborhoodPCode))
            .WithMessage(L("Neighborhood_Required"));

        RuleFor(x => x.BuildingNumber)
            .NotEmpty().WithMessage(L("BuildingNumber_Required"))
            .Length(5).WithMessage(L("BuildingNumber_5Digits"))
            .Matches(@"^\d{5}$").WithMessage(L("BuildingNumber_DigitsOnly"));

        // Composite-ID format enforced by the handler after pCode normalization.


        RuleFor(x => x.BuildingType)
            .Must(v => vocabService.IsValidCode("building_type", (int)v))
            .WithMessage(L("BuildingType_Invalid"));

        RuleFor(x => x.BuildingStatus)
            .Must(v => vocabService.IsValidCode("building_status", (int)v))
            .WithMessage(L("BuildingStatus_Invalid"));

        RuleFor(x => x.NumberOfPropertyUnits)
            .GreaterThanOrEqualTo(0).WithMessage(L("PropertyUnits_NonNegative"))
            .LessThanOrEqualTo(500).WithMessage(L("PropertyUnits_Max500"));

        RuleFor(x => x.NumberOfApartments)
            .GreaterThanOrEqualTo(0).WithMessage(L("Apartments_NonNegative"))
            .LessThanOrEqualTo(500).WithMessage(L("Apartments_Max500"));

        RuleFor(x => x.NumberOfShops)
            .GreaterThanOrEqualTo(0).WithMessage(L("Shops_NonNegative"))
            .LessThanOrEqualTo(200).WithMessage(L("Shops_Max200"));

        // Apartments + Shops should not exceed total PropertyUnits
        RuleFor(x => x)
            .Must(x => (x.NumberOfApartments + x.NumberOfShops) <= x.NumberOfPropertyUnits)
            .When(x => x.NumberOfPropertyUnits > 0)
            .WithMessage(L("Units_SumExceedsTotal"));

        // FIX: Use decimal suffix (m) to match decimal? property type
        // Syria approximate bounds: Lat 32.0°N - 37.5°N, Lng 35.5°E - 42.5°E

        RuleFor(x => x.Latitude)
            .InclusiveBetween(32.0m, 37.5m)
            .When(x => x.Latitude.HasValue)
            .WithMessage(L("Latitude_SyriaBounds"));

        RuleFor(x => x.Longitude)
            .InclusiveBetween(35.5m, 42.5m)
            .When(x => x.Longitude.HasValue)
            .WithMessage(L("Longitude_SyriaBounds"));

        // Both coordinates must be provided together
        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) ||
                       (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage(L("LatLng_BothRequired"));


        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage(L("Notes_MaxLength2000"));
    }
}
