using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.DeleteHouseholdInSurvey;

public class DeleteHouseholdInSurveyCommandValidator : AbstractValidator<DeleteHouseholdInSurveyCommand>
{
    public DeleteHouseholdInSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage("Survey ID is required.");

        RuleFor(x => x.HouseholdId)
            .NotEmpty().WithMessage("Household ID is required.");
    }
}
