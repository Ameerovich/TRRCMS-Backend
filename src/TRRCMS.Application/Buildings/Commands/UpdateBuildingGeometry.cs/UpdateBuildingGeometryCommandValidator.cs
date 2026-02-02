using FluentValidation;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuildingGeometry;

public class UpdateBuildingGeometryCommandValidator : AbstractValidator<UpdateBuildingGeometryCommand>
{
    public UpdateBuildingGeometryCommandValidator()
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage("Building ID is required");

        // Coordinate validations (Syria bounds)
        RuleFor(x => x.Latitude)
            .InclusiveBetween(32.0m, 37.5m)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between 32.0 and 37.5 (Syria bounds)");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(36.0m, 42.5m)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between 36.0 and 42.5 (Syria bounds)");

        // Both coordinates must be provided together
        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) ||
                       (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage("Both latitude and longitude must be provided together");

        // Geometry WKT basic format validation
        RuleFor(x => x.GeometryWkt)
            .MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.GeometryWkt))
            .WithMessage("Geometry WKT must not exceed 10,000 characters");

        RuleFor(x => x.GeometryWkt)
            .Must(BeValidWktFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.GeometryWkt))
            .WithMessage("Geometry WKT must be in valid format (POINT, POLYGON, etc.)");

        // At least one field must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.GeometryWkt) ||
                       (x.Latitude.HasValue && x.Longitude.HasValue))
            .WithMessage("Either GeometryWkt or Latitude/Longitude must be provided");
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