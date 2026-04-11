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
        RuleFor(x => x.GovernorateCode)
            .NotEmpty().WithMessage(L("Governorate_Required"))
            .Length(2).WithMessage(L("Governorate_2Digits"))
            .Matches(@"^\d{2}$").WithMessage(L("Governorate_DigitsOnly"));

        RuleFor(x => x.DistrictCode)
            .NotEmpty().WithMessage(L("District_Required"))
            .Length(2).WithMessage(L("District_2Digits"))
            .Matches(@"^\d{2}$").WithMessage(L("District_DigitsOnly"));

        RuleFor(x => x.SubDistrictCode)
            .NotEmpty().WithMessage(L("SubDistrict_Required"))
            .Length(2).WithMessage(L("SubDistrict_2Digits"))
            .Matches(@"^\d{2}$").WithMessage(L("SubDistrict_DigitsOnly"));

        RuleFor(x => x.CommunityCode)
            .NotEmpty().WithMessage(L("Community_Required"))
            .Length(3).WithMessage(L("Community_3Digits"))
            .Matches(@"^\d{3}$").WithMessage(L("Community_DigitsOnly"));

        RuleFor(x => x.NeighborhoodCode)
            .NotEmpty().WithMessage(L("Neighborhood_Required"))
            .Length(3).WithMessage(L("Neighborhood_3Digits"))
            .Matches(@"^\d{3}$").WithMessage(L("Neighborhood_DigitsOnly"));

        RuleFor(x => x.BuildingNumber)
            .NotEmpty().WithMessage(L("BuildingNumber_Required"))
            .Length(5).WithMessage(L("BuildingNumber_5Digits"))
            .Matches(@"^\d{5}$").WithMessage(L("BuildingNumber_DigitsOnly"));

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
            .WithMessage(L("BuildingId_CompositeFormat"));


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
