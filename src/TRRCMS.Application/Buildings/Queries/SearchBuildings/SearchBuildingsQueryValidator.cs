using FluentValidation;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Buildings.Queries.SearchBuildings;

/// <summary>
/// Validator for SearchBuildingsQuery
/// Validates filters, pagination, and administrative code formats
/// </summary>
public class SearchBuildingsQueryValidator : LocalizedValidator<SearchBuildingsQuery>
{
    private static readonly string[] AllowedSortFields =
        { "buildingid", "createddate", "status", "buildingtype" };

    public SearchBuildingsQueryValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.GovernorateCode)
            .Matches(@"^\d{2}$").WithMessage(L("Governorate_Exactly2Digits"))
            .When(x => !string.IsNullOrWhiteSpace(x.GovernorateCode));

        RuleFor(x => x.DistrictCode)
            .Matches(@"^\d{2}$").WithMessage(L("District_Exactly2Digits"))
            .When(x => !string.IsNullOrWhiteSpace(x.DistrictCode));

        RuleFor(x => x.SubDistrictCode)
            .Matches(@"^\d{2}$").WithMessage(L("SubDistrict_Exactly2Digits"))
            .When(x => !string.IsNullOrWhiteSpace(x.SubDistrictCode));

        RuleFor(x => x.CommunityCode)
            .Matches(@"^\d{3}$").WithMessage(L("Community_Exactly3Digits"))
            .When(x => !string.IsNullOrWhiteSpace(x.CommunityCode));

        RuleFor(x => x.NeighborhoodCode)
            .Matches(@"^\d{3}$").WithMessage(L("Neighborhood_Exactly3Digits"))
            .When(x => !string.IsNullOrWhiteSpace(x.NeighborhoodCode));


        RuleFor(x => x.BuildingId)
            .Matches(@"^\d+$").WithMessage(L("BuildingId_DigitsOnly"))
            .MaximumLength(17).WithMessage(L("BuildingId_MaxLength17"))
            .When(x => !string.IsNullOrWhiteSpace(x.BuildingId));

        RuleFor(x => x.BuildingNumber)
            .Matches(@"^\d+$").WithMessage(L("BuildingNumber_DigitsOnly"))
            .MaximumLength(5).WithMessage(L("BuildingNumber_MaxLength5"))
            .When(x => !string.IsNullOrWhiteSpace(x.BuildingNumber));


        RuleFor(x => x.Status)
            .Must(v => vocabService.IsValidCode("building_status", (int)v!.Value))
            .When(x => x.Status.HasValue)
            .WithMessage(L("BuildingStatus_Invalid"));

        RuleFor(x => x.BuildingType)
            .Must(v => vocabService.IsValidCode("building_type", (int)v!.Value))
            .When(x => x.BuildingType.HasValue)
            .WithMessage(L("BuildingType_Invalid"));


        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage(L("Page_AtLeast1"));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(L("PageSize_Between1And100"));


        RuleFor(x => x.SortBy)
            .Must(sortBy => AllowedSortFields.Contains(sortBy!.ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage(L("SortField_Invalid"));

        // District requires Governorate, SubDistrict requires District, etc.

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.GovernorateCode) || string.IsNullOrWhiteSpace(x.DistrictCode))
            .WithMessage(L("District_RequiresGovernorate"));

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.DistrictCode) || string.IsNullOrWhiteSpace(x.SubDistrictCode))
            .WithMessage(L("SubDistrict_RequiresDistrict"));

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.SubDistrictCode) || string.IsNullOrWhiteSpace(x.CommunityCode))
            .WithMessage(L("Community_RequiresSubDistrict"));
    }
}
