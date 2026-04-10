using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Surveys.Commands.UpdateHouseholdInSurvey;

/// <summary>
/// Validator for UpdateHouseholdInSurveyCommand — all fields optional (canonical v1.9 shape).
/// Upper-bound only rules.
/// </summary>
public class UpdateHouseholdInSurveyCommandValidator : LocalizedValidator<UpdateHouseholdInSurveyCommand>
{
    public UpdateHouseholdInSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.HouseholdId)
            .NotEmpty()
            .WithMessage(L("HouseholdId_Invalid"));

        RuleFor(x => x.HouseholdSize!.Value)
            .GreaterThan(0).WithMessage(L("HouseholdSize_AtLeast1"))
            .LessThanOrEqualTo(50).WithMessage(L("HouseholdSize_Max50"))
            .When(x => x.HouseholdSize.HasValue);

        RuleFor(x => x.MaleCount!.Value)
            .GreaterThanOrEqualTo(0).WithMessage(L("Male_NonNegative"))
            .LessThanOrEqualTo(50).WithMessage(L("Male_Max50"))
            .When(x => x.MaleCount.HasValue);

        RuleFor(x => x.FemaleCount!.Value)
            .GreaterThanOrEqualTo(0).WithMessage(L("Female_NonNegative"))
            .LessThanOrEqualTo(50).WithMessage(L("Female_Max50"))
            .When(x => x.FemaleCount.HasValue);

        RuleFor(x => x.AdultCount!.Value)
            .GreaterThanOrEqualTo(0).WithMessage(L("AdultCount_NonNegative"))
            .LessThanOrEqualTo(50).WithMessage(L("AdultCount_Max50"))
            .When(x => x.AdultCount.HasValue);

        RuleFor(x => x.ChildCount!.Value)
            .GreaterThanOrEqualTo(0).WithMessage(L("ChildCount_NonNegative"))
            .LessThanOrEqualTo(50).WithMessage(L("ChildCount_Max50"))
            .When(x => x.ChildCount.HasValue);

        RuleFor(x => x.ElderlyCount!.Value)
            .GreaterThanOrEqualTo(0).WithMessage(L("ElderlyCount_NonNegative"))
            .LessThanOrEqualTo(50).WithMessage(L("ElderlyCount_Max50"))
            .When(x => x.ElderlyCount.HasValue);

        RuleFor(x => x.DisabledCount!.Value)
            .GreaterThanOrEqualTo(0).WithMessage(L("DisabledCount_NonNegative"))
            .LessThanOrEqualTo(50).WithMessage(L("DisabledCount_Max50"))
            .When(x => x.DisabledCount.HasValue);

        // Cross-field checks — only when HouseholdSize is in the request
        RuleFor(x => x)
            .Must(x => (x.MaleCount ?? 0) + (x.FemaleCount ?? 0) <= x.HouseholdSize!.Value)
            .WithMessage(L("Gender_SumExceedsHouseholdSize"))
            .When(x => x.HouseholdSize.HasValue && (x.MaleCount.HasValue || x.FemaleCount.HasValue));

        RuleFor(x => x)
            .Must(x => (x.AdultCount ?? 0) + (x.ChildCount ?? 0) + (x.ElderlyCount ?? 0) <= x.HouseholdSize!.Value)
            .WithMessage(L("Age_SumExceedsHouseholdSize"))
            .When(x => x.HouseholdSize.HasValue &&
                       (x.AdultCount.HasValue || x.ChildCount.HasValue || x.ElderlyCount.HasValue))
;
        RuleFor(x => x)
            .Must(x => (x.DisabledCount ?? 0) <= x.HouseholdSize!.Value)
            .WithMessage(L("Disabled_ExceedsHouseholdSize"))
            .When(x => x.HouseholdSize.HasValue && x.DisabledCount.HasValue);

        // Occupancy Nature — validate against vocabulary
        RuleFor(x => x.OccupancyNature)
            .Must(v => vocabService.IsValidCode("occupancy_nature", v!.Value))
            .When(x => x.OccupancyNature.HasValue)
            .WithMessage(L("OccupancyNature_Invalid"));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage(L("Notes_MaxLength2000"));
    }
}
