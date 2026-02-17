using FluentValidation;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsInPolygon;

/// <summary>
/// Validator for GetBuildingsInPolygonQuery
/// Validates polygon input (WKT or coordinates), pagination, and filters
/// </summary>
public class GetBuildingsInPolygonQueryValidator : AbstractValidator<GetBuildingsInPolygonQuery>
{
    public GetBuildingsInPolygonQueryValidator(IVocabularyValidationService vocabService)
    {
        // ==================== POLYGON INPUT ====================
        // Either PolygonWkt or Coordinates must be provided

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.PolygonWkt) ||
                        (x.Coordinates != null && x.Coordinates.Length >= 3))
            .WithMessage("Either PolygonWkt or Coordinates (minimum 3 points) must be provided");

        // WKT format validation
        RuleFor(x => x.PolygonWkt)
            .Must(BeValidPolygonWkt)
            .When(x => !string.IsNullOrWhiteSpace(x.PolygonWkt))
            .WithMessage("PolygonWkt must be a valid POLYGON WKT format (e.g., 'POLYGON((lng1 lat1, lng2 lat2, ...))')");

        RuleFor(x => x.PolygonWkt)
            .MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.PolygonWkt))
            .WithMessage("PolygonWkt must not exceed 10,000 characters");

        // Coordinates array validation
        RuleFor(x => x.Coordinates)
            .Must(coords => coords!.Length >= 3)
            .When(x => x.Coordinates != null)
            .WithMessage("Polygon must have at least 3 coordinate points");

        RuleFor(x => x.Coordinates)
            .Must(coords => coords!.Length <= 10000)
            .When(x => x.Coordinates != null)
            .WithMessage("Polygon cannot exceed 10,000 coordinate points");

        RuleFor(x => x.Coordinates)
            .Must(coords => coords!.All(c => c != null && c.Length == 2))
            .When(x => x.Coordinates != null)
            .WithMessage("Each coordinate must be an array of exactly 2 values [longitude, latitude]");

        RuleFor(x => x.Coordinates)
            .Must(BeValidCoordinateValues)
            .When(x => x.Coordinates != null && x.Coordinates.All(c => c != null && c.Length == 2))
            .WithMessage("Coordinates must be within valid ranges (longitude: 35.0-43.0, latitude: 32.0-37.5 for Syria)");

        // ==================== ENUM FILTERS ====================

        RuleFor(x => x.BuildingType)
            .Must(v => vocabService.IsValidCode("building_type", (int)v!.Value))
            .When(x => x.BuildingType.HasValue)
            .WithMessage("Invalid building type value");

        RuleFor(x => x.Status)
            .Must(v => vocabService.IsValidCode("building_status", (int)v!.Value))
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid building status value");

        RuleFor(x => x.DamageLevel)
            .Must(v => vocabService.IsValidCode("damage_level", (int)v!.Value))
            .When(x => x.DamageLevel.HasValue)
            .WithMessage("Invalid damage level value");

        // ==================== PAGINATION ====================

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000).WithMessage("Page size must be between 1 and 1000");
    }

    /// <summary>
    /// Basic WKT POLYGON format validation
    /// </summary>
    private bool BeValidPolygonWkt(string? wkt)
    {
        if (string.IsNullOrWhiteSpace(wkt))
            return true;

        var upper = wkt.Trim().ToUpperInvariant();
        return upper.StartsWith("POLYGON") && upper.Contains("(") && upper.Contains(")");
    }

    /// <summary>
    /// Validate coordinate values are within Syria bounds
    /// </summary>
    private bool BeValidCoordinateValues(double[][]? coords)
    {
        if (coords == null) return true;

        foreach (var c in coords)
        {
            if (c == null || c.Length != 2) return false;

            var lng = c[0];
            var lat = c[1];

            // Extended Syria bounds with buffer
            if (lng < 35.0 || lng > 43.0 || lat < 32.0 || lat > 37.5)
                return false;
        }

        return true;
    }
}
