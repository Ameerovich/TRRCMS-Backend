using FluentValidation;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuilding;

/// <summary>
/// Validator for UpdateBuildingCommand
/// Enhanced with Syria geographic bounds and upper unit limits
/// </summary>
public class UpdateBuildingCommandValidator : LocalizedValidator<UpdateBuildingCommand>
{
    public UpdateBuildingCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage(L("Building_ToggleLockRequired"));

        // Building type validation
        RuleFor(x => x.BuildingType)
            .Must(v => vocabService.IsValidCode("building_type", (int)v!.Value))
            .When(x => x.BuildingType.HasValue)
            .WithMessage(L("BuildingType_Invalid"));

        // Building status validation
        RuleFor(x => x.BuildingStatus)
            .Must(v => vocabService.IsValidCode("building_status", (int)v!.Value))
            .When(x => x.BuildingStatus.HasValue)
            .WithMessage(L("BuildingStatus_Invalid"));

        // Unit count validations (with upper limits)
        RuleFor(x => x.NumberOfPropertyUnits)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfPropertyUnits.HasValue)
            .WithMessage(L("PropertyUnits_NonNegative"));

        RuleFor(x => x.NumberOfPropertyUnits)
            .LessThanOrEqualTo(500)
            .When(x => x.NumberOfPropertyUnits.HasValue)
            .WithMessage(L("PropertyUnits_Max500"));

        RuleFor(x => x.NumberOfApartments)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfApartments.HasValue)
            .WithMessage(L("Apartments_NonNegative"));

        RuleFor(x => x.NumberOfApartments)
            .LessThanOrEqualTo(500)
            .When(x => x.NumberOfApartments.HasValue)
            .WithMessage(L("Apartments_Max500"));

        RuleFor(x => x.NumberOfShops)
            .GreaterThanOrEqualTo(0)
            .When(x => x.NumberOfShops.HasValue)
            .WithMessage(L("Shops_NonNegative"));

        RuleFor(x => x.NumberOfShops)
            .LessThanOrEqualTo(200)
            .When(x => x.NumberOfShops.HasValue)
            .WithMessage(L("Shops_Max200"));

        // Syria geographic bounds for coordinates
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

        // Description validations
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage(L("Notes_MaxLength2000"));
    }
}
