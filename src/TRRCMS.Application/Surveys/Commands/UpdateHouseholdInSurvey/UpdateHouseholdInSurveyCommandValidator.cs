using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UpdateHouseholdInSurvey;

/// <summary>
/// Validator for UpdateHouseholdInSurveyCommand
/// </summary>
public class UpdateHouseholdInSurveyCommandValidator : AbstractValidator<UpdateHouseholdInSurveyCommand>
{
    public UpdateHouseholdInSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.HouseholdId)
            .NotEmpty()
            .WithMessage("Household ID is required");

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

        // Notes
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes != null);
    }
}
