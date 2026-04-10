using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.CancelSurvey;

public class CancelSurveyCommandValidator : LocalizedValidator<CancelSurveyCommand>
{
    public CancelSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(L("CancellationReason_Required"))
            .MaximumLength(500).WithMessage(L("Reason_MaxLength500"));
    }
}
