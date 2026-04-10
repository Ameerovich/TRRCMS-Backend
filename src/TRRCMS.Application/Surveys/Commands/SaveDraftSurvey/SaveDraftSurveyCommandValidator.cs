using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Surveys.Commands.SaveDraftSurvey;

/// <summary>
/// Validator for SaveDraftSurveyCommand
/// </summary>
public class SaveDraftSurveyCommandValidator : LocalizedValidator<SaveDraftSurveyCommand>
{
    public SaveDraftSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.GpsCoordinates)
            .Must(BeValidGpsCoordinates)
            .When(x => !string.IsNullOrWhiteSpace(x.GpsCoordinates))
            .WithMessage(L("GpsFormat_Invalid"));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage(L("Notes_MaxLength2000"));

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage(L("Duration_GreaterThanZero"))
            .LessThanOrEqualTo(480) // 8 hours max
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage(L("Duration_Max480"));
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
