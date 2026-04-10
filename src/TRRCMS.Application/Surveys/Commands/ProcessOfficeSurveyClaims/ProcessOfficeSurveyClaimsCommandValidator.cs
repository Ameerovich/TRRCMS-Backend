using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.ProcessOfficeSurveyClaims;

/// <summary>
/// Validator for ProcessOfficeSurveyClaimsCommand.
/// Same validation rules as the original FinalizeOfficeSurveyCommandValidator.
/// </summary>
public class ProcessOfficeSurveyClaimsCommandValidator : LocalizedValidator<ProcessOfficeSurveyClaimsCommand>
{
    public ProcessOfficeSurveyClaimsCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.FinalNotes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.FinalNotes))
            .WithMessage(L("FinalNotes_MaxLength2000"));

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage(L("Duration_GreaterThanZero"))
            .LessThanOrEqualTo(1440) // Max 24 hours
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage(L("Duration_Max1440"));
    }
}
