using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.FinalizeFieldSurvey;

/// <summary>
/// Validator for FinalizeFieldSurveyCommand
/// Validates input before processing
/// </summary>
public class FinalizeFieldSurveyCommandValidator : AbstractValidator<FinalizeFieldSurveyCommand>
{
    public FinalizeFieldSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required.");

        RuleFor(x => x.FinalNotes)
            .MaximumLength(4000)
            .WithMessage("Final notes cannot exceed 4000 characters.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be greater than 0 minutes.");

        RuleFor(x => x.DurationMinutes)
            .LessThanOrEqualTo(1440) // 24 hours max
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration cannot exceed 1440 minutes (24 hours).");

        RuleFor(x => x.FinalGpsCoordinates)
            .MaximumLength(100)
            .WithMessage("GPS coordinates cannot exceed 100 characters.")
            .Matches(@"^-?\d+\.?\d*,-?\d+\.?\d*$")
            .When(x => !string.IsNullOrWhiteSpace(x.FinalGpsCoordinates))
            .WithMessage("GPS coordinates must be in format 'latitude,longitude' (e.g., '36.2021,37.1343').");
    }
}