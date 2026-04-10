using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Households.Commands.CreateHousehold;

/// <summary>
/// Validator for CreateHouseholdCommand (canonical v1.9 shape).
/// Upper-bound only rules — gaps are allowed for unknown members.
/// </summary>
public class CreateHouseholdCommandValidator : LocalizedValidator<CreateHouseholdCommand>
{
    public CreateHouseholdCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage(L("PropertyUnitId_Required"));

        RuleFor(x => x.HouseholdSize)
            .GreaterThan(0).WithMessage(L("HouseholdSize_AtLeast1"))
            .LessThanOrEqualTo(50).WithMessage(L("HouseholdSize_Max50"));

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

        // Cross-field upper-bound checks
        RuleFor(x => x)
            .Must(x => (x.MaleCount ?? 0) + (x.FemaleCount ?? 0) <= x.HouseholdSize)
            .WithMessage(L("Gender_SumExceedsHouseholdSize"))
            .When(x => x.MaleCount.HasValue || x.FemaleCount.HasValue);

        RuleFor(x => x)
            .Must(x => (x.AdultCount ?? 0) + (x.ChildCount ?? 0) + (x.ElderlyCount ?? 0) <= x.HouseholdSize)
            .WithMessage(L("Age_SumExceedsHouseholdSize"))
            .When(x => x.AdultCount.HasValue || x.ChildCount.HasValue || x.ElderlyCount.HasValue);

        RuleFor(x => x)
            .Must(x => (x.DisabledCount ?? 0) <= x.HouseholdSize)
            .WithMessage(L("Disabled_ExceedsHouseholdSize"))
            .When(x => x.DisabledCount.HasValue);

        RuleFor(x => x.OccupancyNature!.Value)
            .Must(v => Enum.IsDefined(typeof(OccupancyNature), v))
            .WithMessage(L("OccupancyNature_Invalid"))
            .When(x => x.OccupancyNature.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage(L("Notes_MaxLength2000"));
    }
}
