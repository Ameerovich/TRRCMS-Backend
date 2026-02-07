using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.FinalizeOfficeSurvey;

/// <summary>
/// Validator for FinalizeOfficeSurveyCommand.
/// Only SurveyId is required.
/// </summary>
public class FinalizeOfficeSurveyCommandValidator : AbstractValidator<FinalizeOfficeSurveyCommand>
{
    public FinalizeOfficeSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");
    }
}
