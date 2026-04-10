using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Households.Commands.UpdateHousehold;

/// <summary>
/// Validator for UpdateHouseholdCommand
/// </summary>
public class UpdateHouseholdCommandValidator : LocalizedValidator<UpdateHouseholdCommand>
{
    public UpdateHouseholdCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Id)
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
            .WithMessage(L("AdultMale_NonNegative"))
            .LessThanOrEqualTo(50)
            .When(x => x.MaleCount.HasValue)
            .WithMessage(L("AdultMale_Max50"));

        RuleFor(x => x.FemaleCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleCount.HasValue)
            .WithMessage(L("AdultFemale_NonNegative"))
            .LessThanOrEqualTo(50)
            .When(x => x.FemaleCount.HasValue)
            .WithMessage(L("AdultFemale_Max50"));

        // Children
        RuleFor(x => x.MaleChildCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleChildCount.HasValue)
            .WithMessage(L("ChildrenMale_NonNegative"))
            .LessThanOrEqualTo(30)
            .When(x => x.MaleChildCount.HasValue)
            .WithMessage(L("ChildrenMale_Max30"));

        RuleFor(x => x.FemaleChildCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleChildCount.HasValue)
            .WithMessage(L("ChildrenFemale_NonNegative"))
            .LessThanOrEqualTo(30)
            .When(x => x.FemaleChildCount.HasValue)
            .WithMessage(L("ChildrenFemale_Max30"));

        // Elderly
        RuleFor(x => x.MaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleElderlyCount.HasValue)
            .WithMessage(L("ElderlyMale_NonNegative"))
            .LessThanOrEqualTo(20)
            .When(x => x.MaleElderlyCount.HasValue)
            .WithMessage(L("ElderlyMale_Max20"));

        RuleFor(x => x.FemaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleElderlyCount.HasValue)
            .WithMessage(L("ElderlyFemale_NonNegative"))
            .LessThanOrEqualTo(20)
            .When(x => x.FemaleElderlyCount.HasValue)
            .WithMessage(L("ElderlyFemale_Max20"));

        // Disabled
        RuleFor(x => x.MaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleDisabledCount.HasValue)
            .WithMessage(L("DisabledMale_NonNegative"))
            .LessThanOrEqualTo(20)
            .When(x => x.MaleDisabledCount.HasValue)
            .WithMessage(L("DisabledMale_Max20"));

        RuleFor(x => x.FemaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleDisabledCount.HasValue)
            .WithMessage(L("DisabledFemale_NonNegative"))
            .LessThanOrEqualTo(20)
            .When(x => x.FemaleDisabledCount.HasValue)
            .WithMessage(L("DisabledFemale_Max20"));

        // Notes
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes != null)
            .WithMessage(L("Notes_MaxLength2000"));
    }
}
