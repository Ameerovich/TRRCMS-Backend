using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Surveys.Commands.DeleteHouseholdInSurvey;

public class DeleteHouseholdInSurveyCommandValidator : LocalizedValidator<DeleteHouseholdInSurveyCommand>
{
    public DeleteHouseholdInSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.HouseholdId)
            .NotEmpty().WithMessage(L("HouseholdId_Required"));
    }
}
