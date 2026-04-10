using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;

/// <summary>
/// Validator for CreateHouseholdInSurveyCommand
/// Enhanced with cross-field demographics consistency validation
/// </summary>
public class CreateHouseholdInSurveyCommandValidator : LocalizedValidator<CreateHouseholdInSurveyCommand>
{
    public CreateHouseholdInSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.HouseholdSize)
            .GreaterThan(0)
            .WithMessage(L("HouseholdSize_AtLeast1"))
            .LessThanOrEqualTo(50)
            .WithMessage(L("HouseholdSize_Max50"));

        // Adults
        RuleFor(x => x.MaleCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L("AdultMale_NonNegative"))
            .LessThanOrEqualTo(50)
            .WithMessage(L("AdultMale_Max50"));

        RuleFor(x => x.FemaleCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L("AdultFemale_NonNegative"))
            .LessThanOrEqualTo(50)
            .WithMessage(L("AdultFemale_Max50"));

        // Children
        RuleFor(x => x.MaleChildCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L("ChildrenMale_NonNegative"))
            .LessThanOrEqualTo(30)
            .WithMessage(L("ChildrenMale_Max30"));

        RuleFor(x => x.FemaleChildCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L("ChildrenFemale_NonNegative"))
            .LessThanOrEqualTo(30)
            .WithMessage(L("ChildrenFemale_Max30"));

        // Elderly
        RuleFor(x => x.MaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L("ElderlyMale_NonNegative"))
            .LessThanOrEqualTo(20)
            .WithMessage(L("ElderlyMale_Max20"));

        RuleFor(x => x.FemaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L("ElderlyFemale_NonNegative"))
            .LessThanOrEqualTo(20)
            .WithMessage(L("ElderlyFemale_Max20"));

        // Disabled
        RuleFor(x => x.MaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L("DisabledMale_NonNegative"))
            .LessThanOrEqualTo(20)
            .WithMessage(L("DisabledMale_Max20"));

        RuleFor(x => x.FemaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(L("DisabledFemale_NonNegative"))
            .LessThanOrEqualTo(20)
            .WithMessage(L("DisabledFemale_Max20"));

        RuleFor(x => x)
            .Must(x =>
            {
                var totalMembers = x.MaleCount + x.FemaleCount +
                                   x.MaleChildCount + x.FemaleChildCount +
                                   x.MaleElderlyCount + x.FemaleElderlyCount;
                return totalMembers <= x.HouseholdSize;
            })
            .WithMessage(L("Demographics_ExceedHouseholdSize"))
            .When(x => x.HouseholdSize > 0);

        RuleFor(x => x)
            .Must(x =>
            {
                var totalDisabled = x.MaleDisabledCount + x.FemaleDisabledCount;
                return totalDisabled <= x.HouseholdSize;
            })
            .WithMessage(L("Disabled_ExceedHouseholdSize"))
            .When(x => x.HouseholdSize > 0);

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
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage(L("Notes_MaxLength2000"));
    }
}
