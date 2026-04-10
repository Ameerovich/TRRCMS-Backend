using FluentValidation;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsInPolygon;

/// <summary>
/// Validator for GetBuildingsInPolygonQuery
/// Validates polygon input (WKT or coordinates), pagination, and filters
/// </summary>
public class GetBuildingsInPolygonQueryValidator : LocalizedValidator<GetBuildingsInPolygonQuery>
{
    public GetBuildingsInPolygonQueryValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        // Either PolygonWkt or Coordinates must be provided

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.PolygonWkt) ||
                        (x.Coordinates != null && x.Coordinates.Length >= 3))
            .WithMessage(L("PolygonOrCoords_Required"));

        // WKT format validation
        RuleFor(x => x.PolygonWkt)
            .Must(BeValidPolygonWkt)
            .When(x => !string.IsNullOrWhiteSpace(x.PolygonWkt))
            .WithMessage(L("PolygonWkt_InvalidFormat"));

        RuleFor(x => x.PolygonWkt)
            .MaximumLength(10000)
            .When(x => !string.IsNullOrWhiteSpace(x.PolygonWkt))
            .WithMessage(L("PolygonWkt_MaxLength10000"));

        // Coordinates array validation
        RuleFor(x => x.Coordinates)
            .Must(coords => coords!.Length >= 3)
            .When(x => x.Coordinates != null)
            .WithMessage(L("Polygon_MinPoints3"));

        RuleFor(x => x.Coordinates)
            .Must(coords => coords!.Length <= 10000)
            .When(x => x.Coordinates != null)
            .WithMessage(L("Polygon_MaxPoints10000"));

        RuleFor(x => x.Coordinates)
            .Must(coords => coords!.All(c => c != null && c.Length == 2))
            .When(x => x.Coordinates != null)
            .WithMessage(L("Coordinate_Exactly2Values"));

        RuleFor(x => x.Coordinates)
            .Must(BeValidCoordinateValues)
            .When(x => x.Coordinates != null && x.Coordinates.All(c => c != null && c.Length == 2))
            .WithMessage(L("Coordinate_SyriaRange"));


        RuleFor(x => x.BuildingType)
            .Must(v => vocabService.IsValidCode("building_type", (int)v!.Value))
            .When(x => x.BuildingType.HasValue)
            .WithMessage(L("BuildingType_Invalid"));

        RuleFor(x => x.Status)
            .Must(v => vocabService.IsValidCode("building_status", (int)v!.Value))
            .When(x => x.Status.HasValue)
            .WithMessage(L("BuildingStatus_Invalid"));


        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage(L("Page_AtLeast1"));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000).WithMessage(L("PageSize_Between1And1000"));
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
