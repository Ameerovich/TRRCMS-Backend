using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;

/// <summary>
/// Validator for GetAllPropertyUnitsQuery
/// Validates filter parameters against vocabulary codes
/// </summary>
public class GetAllPropertyUnitsQueryValidator : LocalizedValidator<GetAllPropertyUnitsQuery>
{
    public GetAllPropertyUnitsQueryValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        // Validate UnitType if provided
        RuleFor(x => x.UnitType)
            .Must(v => vocabService.IsValidCode("property_unit_type", v!.Value))
            .When(x => x.UnitType.HasValue)
            .WithMessage(L("UnitType_Invalid"));

        // Validate Status if provided
        RuleFor(x => x.Status)
            .Must(v => vocabService.IsValidCode("property_unit_status", v!.Value))
            .When(x => x.Status.HasValue)
            .WithMessage(L("UnitStatus_Invalid"));
    }
}
