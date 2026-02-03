using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.SetHouseholdHead;

/// <summary>
/// Validator for SetHouseholdHeadCommand
/// </summary>
public class SetHouseholdHeadCommandValidator : AbstractValidator<SetHouseholdHeadCommand>
{
    public SetHouseholdHeadCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage("Survey ID is required");

        RuleFor(x => x.HouseholdId)
            .NotEmpty().WithMessage("Household ID is required");

        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required");
    }
}
