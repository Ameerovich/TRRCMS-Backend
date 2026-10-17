using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;

namespace TRRCMS.Application.Surveys.Commands.SetSurveyContactPoint;

public class SetSurveyContactPointCommandValidator : LocalizedValidator<SetSurveyContactPointCommand>
{
    public SetSurveyContactPointCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId).NotEmpty().WithMessage(L("SurveyId_Required"));
        RuleFor(x => x.PersonId).NotEmpty().WithMessage(L("PersonId_Required"));
    }
}
