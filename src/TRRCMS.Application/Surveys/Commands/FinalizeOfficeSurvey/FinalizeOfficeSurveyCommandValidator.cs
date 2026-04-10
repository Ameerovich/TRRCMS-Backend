using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.FinalizeOfficeSurvey;

/// <summary>
/// Validator for FinalizeOfficeSurveyCommand.
/// Only SurveyId is required.
/// </summary>
public class FinalizeOfficeSurveyCommandValidator : LocalizedValidator<FinalizeOfficeSurveyCommand>
{
    public FinalizeOfficeSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));
    }
}
