using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;

/// <summary>
/// Validator for CreateHouseholdInSurveyCommand
/// Enhanced with cross-field demographics consistency validation
/// </summary>
public class CreateHouseholdInSurveyCommandValidator : AbstractValidator<CreateHouseholdInSurveyCommand>
{
    public CreateHouseholdInSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

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
            .WithMessage("Adult male count cannot be negative")
            .LessThanOrEqualTo(50)
            .WithMessage("Adult male count must not exceed 50");

        RuleFor(x => x.FemaleCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Adult female count cannot be negative")
            .LessThanOrEqualTo(50)
            .WithMessage("Adult female count must not exceed 50");

        // Children
        RuleFor(x => x.MaleChildCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Male children count cannot be negative")
            .LessThanOrEqualTo(30)
            .WithMessage("Male children count must not exceed 30");

        RuleFor(x => x.FemaleChildCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Female children count cannot be negative")
            .LessThanOrEqualTo(30)
            .WithMessage("Female children count must not exceed 30");

        // Elderly
        RuleFor(x => x.MaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Male elderly count cannot be negative")
            .LessThanOrEqualTo(20)
            .WithMessage("Male elderly count must not exceed 20");

        RuleFor(x => x.FemaleElderlyCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Female elderly count cannot be negative")
            .LessThanOrEqualTo(20)
            .WithMessage("Female elderly count must not exceed 20");

        // Disabled
        RuleFor(x => x.MaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Male disabled count cannot be negative")
            .LessThanOrEqualTo(20)
            .WithMessage("Male disabled count must not exceed 20");

        RuleFor(x => x.FemaleDisabledCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Female disabled count cannot be negative")
            .LessThanOrEqualTo(20)
            .WithMessage("Female disabled count must not exceed 20");

        // ==================== CROSS-FIELD: Demographics sum consistency ====================
        RuleFor(x => x)
            .Must(x =>
            {
                var totalMembers = x.MaleCount + x.FemaleCount +
                                   x.MaleChildCount + x.FemaleChildCount +
                                   x.MaleElderlyCount + x.FemaleElderlyCount;
                return totalMembers <= x.HouseholdSize;
            })
            .WithMessage("Sum of demographic members (adults + children + elderly) cannot exceed household size")
            .When(x => x.HouseholdSize > 0);

        RuleFor(x => x)
            .Must(x =>
            {
                var totalDisabled = x.MaleDisabledCount + x.FemaleDisabledCount;
                return totalDisabled <= x.HouseholdSize;
            })
            .WithMessage("Total disabled count cannot exceed household size")
            .When(x => x.HouseholdSize > 0);

        // Notes
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 2000 characters");
    }
}
