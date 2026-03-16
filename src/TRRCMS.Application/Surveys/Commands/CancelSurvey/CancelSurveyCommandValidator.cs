using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.CancelSurvey;

public class CancelSurveyCommandValidator : AbstractValidator<CancelSurveyCommand>
{
    public CancelSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage("Survey ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
