using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.RevertSurveyToDraft;

public class RevertSurveyToDraftCommandValidator : AbstractValidator<RevertSurveyToDraftCommand>
{
    public RevertSurveyToDraftCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage("Survey ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("A reason for reverting to draft is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
