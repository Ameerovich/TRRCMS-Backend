using FluentValidation;

namespace TRRCMS.Application.Households.Commands.UpdateHousehold;

/// <summary>
/// Validator for UpdateHouseholdCommand
/// </summary>
public class UpdateHouseholdCommandValidator : AbstractValidator<UpdateHouseholdCommand>
{
    public UpdateHouseholdCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Household ID is required");

        RuleFor(x => x.HeadOfHouseholdName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.HeadOfHouseholdName))
            .WithMessage("Head of household name must not exceed 200 characters");

        RuleFor(x => x.HouseholdSize)
            .GreaterThan(0)
            .When(x => x.HouseholdSize.HasValue)
            .WithMessage("Household size must be at least 1")
            .LessThanOrEqualTo(50)
            .When(x => x.HouseholdSize.HasValue)
            .WithMessage("Household size must not exceed 50");

        // Adults
        RuleFor(x => x.MaleCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleCount.HasValue)
            .WithMessage("Adult male count cannot be negative")
            .LessThanOrEqualTo(50)
            .When(x => x.MaleCount.HasValue)
            .WithMessage("Adult male count must not exceed 50");

        RuleFor(x => x.FemaleCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleCount.HasValue)
            .WithMessage("Adult female count cannot be negative")
            .LessThanOrEqualTo(50)
            .When(x => x.FemaleCount.HasValue)
            .WithMessage("Adult female count must not exceed 50");

        // Children
        RuleFor(x => x.MaleChildCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleChildCount.HasValue)
            .WithMessage("Male children count cannot be negative")
            .LessThanOrEqualTo(30)
            .When(x => x.MaleChildCount.HasValue)
            .WithMessage("Male children count must not exceed 30");

        RuleFor(x => x.FemaleChildCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleChildCount.HasValue)
            .WithMessage("Female children count cannot be negative")
            .LessThanOrEqualTo(30)
            .When(x => x.FemaleChildCount.HasValue)
            .WithMessage("Female children count must not exceed 30");

        // Elderly
        RuleFor(x => x.MaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleElderlyCount.HasValue)
            .WithMessage("Male elderly count cannot be negative")
            .LessThanOrEqualTo(20)
            .When(x => x.MaleElderlyCount.HasValue)
            .WithMessage("Male elderly count must not exceed 20");

        RuleFor(x => x.FemaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleElderlyCount.HasValue)
            .WithMessage("Female elderly count cannot be negative")
            .LessThanOrEqualTo(20)
            .When(x => x.FemaleElderlyCount.HasValue)
            .WithMessage("Female elderly count must not exceed 20");

        // Disabled
        RuleFor(x => x.MaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleDisabledCount.HasValue)
            .WithMessage("Male disabled count cannot be negative")
            .LessThanOrEqualTo(20)
            .When(x => x.MaleDisabledCount.HasValue)
            .WithMessage("Male disabled count must not exceed 20");

        RuleFor(x => x.FemaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleDisabledCount.HasValue)
            .WithMessage("Female disabled count cannot be negative")
            .LessThanOrEqualTo(20)
            .When(x => x.FemaleDisabledCount.HasValue)
            .WithMessage("Female disabled count must not exceed 20");

        // Notes
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes != null)
            .WithMessage("Notes must not exceed 2000 characters");
    }
}
