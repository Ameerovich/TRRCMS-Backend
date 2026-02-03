using FluentValidation;

namespace TRRCMS.Application.Buildings.Queries.SearchBuildings;

/// <summary>
/// Validator for SearchBuildingsQuery
/// Validates filters, pagination, and administrative code formats
/// </summary>
public class SearchBuildingsQueryValidator : AbstractValidator<SearchBuildingsQuery>
{
    private static readonly string[] AllowedSortFields =
        { "buildingid", "createddate", "status", "buildingtype" };

    public SearchBuildingsQueryValidator()
    {
        // ==================== ADMINISTRATIVE CODES ====================

        RuleFor(x => x.GovernorateCode)
            .Matches(@"^\d{2}$").WithMessage("Governorate code must be exactly 2 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.GovernorateCode));

        RuleFor(x => x.DistrictCode)
            .Matches(@"^\d{2}$").WithMessage("District code must be exactly 2 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.DistrictCode));

        RuleFor(x => x.SubDistrictCode)
            .Matches(@"^\d{2}$").WithMessage("Sub-district code must be exactly 2 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.SubDistrictCode));

        RuleFor(x => x.CommunityCode)
            .Matches(@"^\d{3}$").WithMessage("Community code must be exactly 3 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.CommunityCode));

        RuleFor(x => x.NeighborhoodCode)
            .Matches(@"^\d{3}$").WithMessage("Neighborhood code must be exactly 3 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.NeighborhoodCode));

        // ==================== DIRECT IDENTIFIERS ====================

        RuleFor(x => x.BuildingId)
            .Matches(@"^\d+$").WithMessage("Building ID must contain only digits")
            .MaximumLength(17).WithMessage("Building ID cannot exceed 17 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.BuildingId));

        RuleFor(x => x.BuildingNumber)
            .Matches(@"^\d+$").WithMessage("Building number must contain only digits")
            .MaximumLength(5).WithMessage("Building number cannot exceed 5 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.BuildingNumber));

        // ==================== ENUM FILTERS ====================

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid building status value")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.BuildingType)
            .IsInEnum().WithMessage("Invalid building type value")
            .When(x => x.BuildingType.HasValue);

        // ==================== PAGINATION ====================

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        // ==================== SORTING ====================

        RuleFor(x => x.SortBy)
            .Must(sortBy => AllowedSortFields.Contains(sortBy!.ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage("Sort field must be one of: buildingId, createdDate, status, buildingType");

        // ==================== HIERARCHY CONSISTENCY ====================
        // District requires Governorate, SubDistrict requires District, etc.

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.GovernorateCode) || string.IsNullOrWhiteSpace(x.DistrictCode))
            .WithMessage("District code requires Governorate code to be specified");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.DistrictCode) || string.IsNullOrWhiteSpace(x.SubDistrictCode))
            .WithMessage("Sub-district code requires District code to be specified");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.SubDistrictCode) || string.IsNullOrWhiteSpace(x.CommunityCode))
            .WithMessage("Community code requires Sub-district code to be specified");
    }
}
