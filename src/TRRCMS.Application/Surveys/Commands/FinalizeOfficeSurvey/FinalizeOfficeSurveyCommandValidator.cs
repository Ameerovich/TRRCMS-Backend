using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.FinalizeOfficeSurvey;

/// <summary>
/// Validator for FinalizeOfficeSurveyCommand
/// UC-004 S21 / UC-005: Finalization validation rules
/// </summary>
public class FinalizeOfficeSurveyCommandValidator : AbstractValidator<FinalizeOfficeSurveyCommand>
{
    public FinalizeOfficeSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.FinalNotes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.FinalNotes))
            .WithMessage("Final notes cannot exceed 2000 characters");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be greater than 0 minutes")
            .LessThanOrEqualTo(1440) // Max 24 hours
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration cannot exceed 1440 minutes (24 hours)");
    }
}
