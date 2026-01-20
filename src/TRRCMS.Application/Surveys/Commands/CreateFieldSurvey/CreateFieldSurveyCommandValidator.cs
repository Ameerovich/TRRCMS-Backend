using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.CreateFieldSurvey;

/// <summary>
/// Validator for CreateFieldSurveyCommand
/// </summary>
public class CreateFieldSurveyCommandValidator : AbstractValidator<CreateFieldSurveyCommand>
{
    public CreateFieldSurveyCommandValidator()
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage("Building ID is required");

        RuleFor(x => x.SurveyDate)
            .NotEmpty()
            .WithMessage("Survey date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Survey date cannot be in the future");

        RuleFor(x => x.GpsCoordinates)
            .Must(BeValidGpsCoordinates)
            .When(x => !string.IsNullOrWhiteSpace(x.GpsCoordinates))
            .WithMessage("GPS coordinates must be in format 'latitude,longitude' (e.g., '36.2021,37.1343')");

        RuleFor(x => x.IntervieweeName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.IntervieweeName))
            .WithMessage("Interviewee name cannot exceed 200 characters");

        RuleFor(x => x.IntervieweeRelationship)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.IntervieweeRelationship))
            .WithMessage("Interviewee relationship cannot exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 2000 characters");
    }

    /// <summary>
    /// Validate GPS coordinates format: "latitude,longitude"
    /// </summary>
    private bool BeValidGpsCoordinates(string? coordinates)
    {
        if (string.IsNullOrWhiteSpace(coordinates))
            return true;

        var parts = coordinates.Split(',');
        if (parts.Length != 2)
            return false;

        if (!decimal.TryParse(parts[0].Trim(), out decimal lat) ||
            !decimal.TryParse(parts[1].Trim(), out decimal lng))
            return false;

        // Validate latitude range: -90 to 90
        if (lat < -90 || lat > 90)
            return false;

        // Validate longitude range: -180 to 180
        if (lng < -180 || lng > 180)
            return false;

        return true;
    }
}