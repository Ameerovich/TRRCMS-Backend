using FluentValidation;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsForMap;

public class GetBuildingsForMapQueryValidator : AbstractValidator<GetBuildingsForMapQuery>
{
    public GetBuildingsForMapQueryValidator()
    {
        // Latitude bounds (Syria: 32.0 to 37.5)
        RuleFor(x => x.NorthEastLat)
            .InclusiveBetween(32.0m, 37.5m)
            .WithMessage("NorthEast Latitude must be between 32.0 and 37.5");

        RuleFor(x => x.SouthWestLat)
            .InclusiveBetween(32.0m, 37.5m)
            .WithMessage("SouthWest Latitude must be between 32.0 and 37.5");

        // Longitude bounds (Syria: 36.0 to 42.5)
        RuleFor(x => x.NorthEastLng)
            .InclusiveBetween(36.0m, 42.5m)
            .WithMessage("NorthEast Longitude must be between 36.0 and 42.5");

        RuleFor(x => x.SouthWestLng)
            .InclusiveBetween(36.0m, 42.5m)
            .WithMessage("SouthWest Longitude must be between 36.0 and 42.5");

        // Bounding box logic validation
        RuleFor(x => x)
            .Must(x => x.NorthEastLat > x.SouthWestLat)
            .WithMessage("NorthEast Latitude must be greater than SouthWest Latitude");

        RuleFor(x => x)
            .Must(x => x.NorthEastLng > x.SouthWestLng)
            .WithMessage("NorthEast Longitude must be greater than SouthWest Longitude");

        // Max results validation
        RuleFor(x => x.MaxResults)
            .GreaterThan(0)
            .WithMessage("MaxResults must be greater than 0")
            .LessThanOrEqualTo(50000)
            .WithMessage("MaxResults cannot exceed 50,000 for performance reasons");
    }
}