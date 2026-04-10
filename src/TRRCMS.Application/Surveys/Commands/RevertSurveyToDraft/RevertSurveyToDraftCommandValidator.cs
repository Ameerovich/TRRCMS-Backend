using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Surveys.Commands.RevertSurveyToDraft;

public class RevertSurveyToDraftCommandValidator : LocalizedValidator<RevertSurveyToDraftCommand>
{
    public RevertSurveyToDraftCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(L("RevertReason_Required"))
            .MaximumLength(500).WithMessage(L("Reason_MaxLength500"));
    }
}
