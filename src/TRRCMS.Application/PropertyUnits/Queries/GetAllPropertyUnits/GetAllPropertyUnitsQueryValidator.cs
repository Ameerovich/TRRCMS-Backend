using FluentValidation;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;

/// <summary>
/// Validator for GetAllPropertyUnitsQuery
/// Validates filter parameters against vocabulary codes
/// </summary>
public class GetAllPropertyUnitsQueryValidator : AbstractValidator<GetAllPropertyUnitsQuery>
{
    public GetAllPropertyUnitsQueryValidator(IVocabularyValidationService vocabService)
    {
        // Validate UnitType if provided
        RuleFor(x => x.UnitType)
            .Must(v => vocabService.IsValidCode("property_unit_type", v!.Value))
            .When(x => x.UnitType.HasValue)
            .WithMessage("Invalid property unit type value");

        // Validate Status if provided
        RuleFor(x => x.Status)
            .Must(v => vocabService.IsValidCode("property_unit_status", v!.Value))
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid property unit status value");
    }
}
