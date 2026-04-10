using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.UpdateHouseholdInSurvey;

/// <summary>
/// Validator for UpdateHouseholdInSurveyCommand
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
            .WithMessage(L("HouseholdId_Required"));

        RuleFor(x => x.HouseholdSize)
            .GreaterThan(0)
            .When(x => x.HouseholdSize.HasValue)
            .WithMessage(L("HouseholdSize_AtLeast1"))
            .LessThanOrEqualTo(50)
            .When(x => x.HouseholdSize.HasValue)
            .WithMessage(L("HouseholdSize_Max50"));

        // Adults
        RuleFor(x => x.MaleCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleCount.HasValue)
            .LessThanOrEqualTo(50)
            .When(x => x.MaleCount.HasValue);

        RuleFor(x => x.FemaleCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleCount.HasValue)
            .LessThanOrEqualTo(50)
            .When(x => x.FemaleCount.HasValue);

        // Children
        RuleFor(x => x.MaleChildCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleChildCount.HasValue)
            .LessThanOrEqualTo(30)
            .When(x => x.MaleChildCount.HasValue);

        RuleFor(x => x.FemaleChildCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleChildCount.HasValue)
            .LessThanOrEqualTo(30)
            .When(x => x.FemaleChildCount.HasValue);

        // Elderly
        RuleFor(x => x.MaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleElderlyCount.HasValue)
            .LessThanOrEqualTo(20)
            .When(x => x.MaleElderlyCount.HasValue);

        RuleFor(x => x.FemaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleElderlyCount.HasValue)
            .LessThanOrEqualTo(20)
            .When(x => x.FemaleElderlyCount.HasValue);

        // Disabled
        RuleFor(x => x.MaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleDisabledCount.HasValue)
            .LessThanOrEqualTo(20)
            .When(x => x.MaleDisabledCount.HasValue);

        RuleFor(x => x.FemaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleDisabledCount.HasValue)
            .LessThanOrEqualTo(20)
            .When(x => x.FemaleDisabledCount.HasValue);

        // Occupancy Type
        RuleFor(x => x.OccupancyType)
            .Must(v => vocabService.IsValidCode("occupancy_type", v!.Value))
            .When(x => x.OccupancyType.HasValue)
            .WithMessage(L("OccupancyType_Invalid"));

        // Occupancy Nature
        RuleFor(x => x.OccupancyNature)
            .Must(v => vocabService.IsValidCode("occupancy_nature", v!.Value))
            .When(x => x.OccupancyNature.HasValue)
            .WithMessage(L("OccupancyNature_Invalid"));

        // Notes
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes != null);
    }
}
