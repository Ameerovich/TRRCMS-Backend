using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.LinkPropertyUnitToSurvey;

/// <summary>
/// Validator for LinkPropertyUnitToSurveyCommand
/// </summary>
public class LinkPropertyUnitToSurveyCommandValidator : LocalizedValidator<LinkPropertyUnitToSurveyCommand>
{
    public LinkPropertyUnitToSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage(L("PropertyUnitId_Required"));
    }
}
