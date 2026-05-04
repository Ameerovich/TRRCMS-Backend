using FluentValidation;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Buildings.Commands.RegisterBuilding;

/// <summary>
/// Validator for RegisterBuildingCommand (QGIS plugin endpoint).
/// Accepts either raw admin codes or OCHA pCodes (or a mix). Format checks
/// only fire when a value is supplied; the level-required rule is satisfied
/// by either side.
/// BuildingGeometryWkt is required (QGIS always provides polygon).
/// </summary>
public class RegisterBuildingCommandValidator : LocalizedValidator<RegisterBuildingCommand>
{
    public RegisterBuildingCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        // Raw-code format checks — only fire when the raw value is provided.
        RuleFor(x => x.GovernorateCode!).Length(2).Matches(@"^\d{2}$")
            .When(x => !string.IsNullOrEmpty(x.GovernorateCode))
            .WithMessage(L("Governorate_Exactly2Digits"));
        RuleFor(x => x.DistrictCode!).Length(2).Matches(@"^\d{2}$")
            .When(x => !string.IsNullOrEmpty(x.DistrictCode))
            .WithMessage(L("District_Exactly2Digits"));
        RuleFor(x => x.SubDistrictCode!).Length(2).Matches(@"^\d{2}$")
            .When(x => !string.IsNullOrEmpty(x.SubDistrictCode))
            .WithMessage(L("SubDistrict_Exactly2Digits"));
        RuleFor(x => x.CommunityCode!).Length(3).Matches(@"^\d{3}$")
            .When(x => !string.IsNullOrEmpty(x.CommunityCode))
            .WithMessage(L("Community_Exactly3Digits"));
        RuleFor(x => x.NeighborhoodCode!).Length(3).Matches(@"^\d{3}$")
            .When(x => !string.IsNullOrEmpty(x.NeighborhoodCode))
            .WithMessage(L("Neighborhood_Exactly3Digits"));

        // pCode format checks — only fire when a pCode is provided.
        RuleFor(x => x.GovernoratePCode!).Matches(@"^(?i)SY\d{2}$")
            .When(x => !string.IsNullOrEmpty(x.GovernoratePCode))
            .WithMessage("GovernoratePCode must look like 'SY02'.");
        RuleFor(x => x.DistrictPCode!).Matches(@"^(?i)SY\d{4}$")
            .When(x => !string.IsNullOrEmpty(x.DistrictPCode))
            .WithMessage("DistrictPCode must look like 'SY0200'.");
        RuleFor(x => x.SubDistrictPCode!).Matches(@"^(?i)SY\d{6}$")
            .When(x => !string.IsNullOrEmpty(x.SubDistrictPCode))
            .WithMessage("SubDistrictPCode must look like 'SY020000'.");
        RuleFor(x => x.CommunityPCode!).Matches(@"^(?i)C\d{1,9}$")
            .When(x => !string.IsNullOrEmpty(x.CommunityPCode))
            .WithMessage("CommunityPCode must look like 'C1007'.");
        RuleFor(x => x.NeighborhoodPCode!).Matches(@"^(?i)N\d{1,9}$")
            .When(x => !string.IsNullOrEmpty(x.NeighborhoodPCode))
            .WithMessage("NeighborhoodPCode must look like 'N0160'.");

        // Each level must be supplied as either raw OR pCode. SubDistrict pCode
        // (SY020000) implicitly satisfies governorate + district too.
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.GovernorateCode) ||
                !string.IsNullOrEmpty(x.GovernoratePCode) ||
                !string.IsNullOrEmpty(x.DistrictPCode) ||
                !string.IsNullOrEmpty(x.SubDistrictPCode))
            .WithMessage(L("Governorate_Required"));
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.DistrictCode) ||
                !string.IsNullOrEmpty(x.DistrictPCode) ||
                !string.IsNullOrEmpty(x.SubDistrictPCode))
            .WithMessage(L("District_Required"));
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.SubDistrictCode) ||
                !string.IsNullOrEmpty(x.SubDistrictPCode))
            .WithMessage(L("SubDistrict_Required"));
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.CommunityCode) ||
                !string.IsNullOrEmpty(x.CommunityPCode))
            .WithMessage(L("Community_Required"));
        RuleFor(x => x).Must(x =>
                !string.IsNullOrEmpty(x.NeighborhoodCode) ||
                !string.IsNullOrEmpty(x.NeighborhoodPCode))
            .WithMessage(L("Neighborhood_Required"));

        RuleFor(x => x.BuildingNumber)
            .NotEmpty().WithMessage(L("BuildingNumber_Required"))
            .Length(5).WithMessage(L("BuildingNumber_5Digits"))
            .Matches(@"^\d{5}$").WithMessage(L("BuildingNumber_DigitsOnly"));

        // Composite-ID format is enforced by the handler after pCode normalization.

        RuleFor(x => x.BuildingGeometryWkt)
            .NotEmpty().WithMessage(L("Building_GeometryRequired"));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage(L("Notes_MaxLength2000"));
    }
}
