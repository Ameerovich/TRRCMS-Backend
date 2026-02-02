using FluentValidation;

namespace TRRCMS.Application.Households.Commands.CreateHousehold;

/// <summary>
/// Validator for CreateHouseholdCommand
/// </summary>
public class CreateHouseholdCommandValidator : AbstractValidator<CreateHouseholdCommand>
{
    public CreateHouseholdCommandValidator()
    {
        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage("Property unit ID is required");

        RuleFor(x => x.HeadOfHouseholdName)
            .NotEmpty()
            .WithMessage("Head of household name (رب الأسرة) is required")
            .MaximumLength(200)
            .WithMessage("Head of household name must not exceed 200 characters");

        RuleFor(x => x.HouseholdSize)
            .GreaterThan(0)
            .WithMessage("Household size (عدد الأفراد) must be at least 1")
            .LessThanOrEqualTo(50)
            .WithMessage("Household size must not exceed 50");

        // Adults
        RuleFor(x => x.MaleCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Adult male count (عدد البالغين الذكور) cannot be negative")
            .LessThanOrEqualTo(50)
            .WithMessage("Adult male count must not exceed 50");

        RuleFor(x => x.FemaleCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Adult female count (عدد البالغين الإناث) cannot be negative")
            .LessThanOrEqualTo(50)
            .WithMessage("Adult female count must not exceed 50");

        // Children
        RuleFor(x => x.MaleChildCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Male children count (عدد الأطفال الذكور) cannot be negative")
            .LessThanOrEqualTo(30)
            .WithMessage("Male children count must not exceed 30");

        RuleFor(x => x.FemaleChildCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Female children count (عدد الأطفال الإناث) cannot be negative")
            .LessThanOrEqualTo(30)
            .WithMessage("Female children count must not exceed 30");

        // Elderly
        RuleFor(x => x.MaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Male elderly count (عدد كبار السن الذكور) cannot be negative")
            .LessThanOrEqualTo(20)
            .WithMessage("Male elderly count must not exceed 20");

        RuleFor(x => x.FemaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Female elderly count (عدد كبار السن الإناث) cannot be negative")
            .LessThanOrEqualTo(20)
            .WithMessage("Female elderly count must not exceed 20");

        // Disabled
        RuleFor(x => x.MaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Male disabled count (عدد المعاقين الذكور) cannot be negative")
            .LessThanOrEqualTo(20)
            .WithMessage("Male disabled count must not exceed 20");

        RuleFor(x => x.FemaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Female disabled count (عدد المعاقين الإناث) cannot be negative")
            .LessThanOrEqualTo(20)
            .WithMessage("Female disabled count must not exceed 20");

        // Notes
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes (ملاحظات) must not exceed 2000 characters");
    }
}
