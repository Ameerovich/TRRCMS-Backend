using FluentValidation;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuildingGeometry;

public class UpdateBuildingGeometryCommandValidator : LocalizedValidator<UpdateBuildingGeometryCommand>
{
    public UpdateBuildingGeometryCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage(L("Building_ToggleLockRequired"));

        // Coordinate validations (Syria bounds)
        RuleFor(x => x.Latitude)
            .InclusiveBetween(32.0m, 37.5m)
            .When(x => x.Latitude.HasValue)
            .WithMessage(L("Latitude_SyriaBounds"));

        RuleFor(x => x.Longitude)
            .InclusiveBetween(36.0m, 42.5m)
            .When(x => x.Longitude.HasValue)
            .WithMessage(L("Longitude_SyriaBounds36"));

        // Both coordinates must be provided together
        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) ||
                       (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage(L("LatLng_BothRequired"));

        // Geometry WKT basic format validation
        RuleFor(x => x.GeometryWkt)
            .MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.GeometryWkt))
            .WithMessage(L("GeometryWkt_MaxLength10000"));

        RuleFor(x => x.GeometryWkt)
            .Must(BeValidWktFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.GeometryWkt))
            .WithMessage(L("GeometryWkt_InvalidFormat"));

        // At least one field must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.GeometryWkt) ||
                       (x.Latitude.HasValue && x.Longitude.HasValue))
            .WithMessage(L("GeometryOrLatLng_Required"));
    }

    private bool BeValidWktFormat(string? wkt)
    {
        if (string.IsNullOrWhiteSpace(wkt))
            return true; // Null is valid (will be handled by other rules)

        // Basic WKT format check - must start with a valid geometry type
        var validTypes = new[] { "POINT", "LINESTRING", "POLYGON", "MULTIPOINT",
                                "MULTILINESTRING", "MULTIPOLYGON", "GEOMETRYCOLLECTION" };

        var upperWkt = wkt.Trim().ToUpperInvariant();
        return validTypes.Any(type => upperWkt.StartsWith(type));
    }
}
