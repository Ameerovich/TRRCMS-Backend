using FluentValidation;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsForMap;

public class GetBuildingsForMapQueryValidator : LocalizedValidator<GetBuildingsForMapQuery>
{
    public GetBuildingsForMapQueryValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        // Latitude bounds (Syria: 32.0 to 37.5)
        RuleFor(x => x.NorthEastLat)
            .InclusiveBetween(32.0m, 37.5m)
            .WithMessage(L("NorthEastLat_SyriaBounds"));

        RuleFor(x => x.SouthWestLat)
            .InclusiveBetween(32.0m, 37.5m)
            .WithMessage(L("SouthWestLat_SyriaBounds"));

        // Longitude bounds (Syria: 36.0 to 42.5)
        RuleFor(x => x.NorthEastLng)
            .InclusiveBetween(36.0m, 42.5m)
            .WithMessage(L("NorthEastLng_SyriaBounds"));

        RuleFor(x => x.SouthWestLng)
            .InclusiveBetween(36.0m, 42.5m)
            .WithMessage(L("SouthWestLng_SyriaBounds"));

        // Bounding box logic validation
        RuleFor(x => x)
            .Must(x => x.NorthEastLat > x.SouthWestLat)
            .WithMessage(L("NorthEastLat_GreaterThanSouthWest"));

        RuleFor(x => x)
            .Must(x => x.NorthEastLng > x.SouthWestLng)
            .WithMessage(L("NorthEastLng_GreaterThanSouthWest"));

        // Max results validation
        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage(L("MaxResults_GreaterThanZero"))
            .LessThanOrEqualTo(50000)
            .WithMessage(L("MaxResults_Max50000"));
    }
}
