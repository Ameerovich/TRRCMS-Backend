using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.LinkPropertyUnitToSurvey;

/// <summary>
/// Validator for LinkPropertyUnitToSurveyCommand
/// </summary>
public class LinkPropertyUnitToSurveyCommandValidator : AbstractValidator<LinkPropertyUnitToSurveyCommand>
{
    public LinkPropertyUnitToSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage("Property unit ID is required");
    }
}