using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.DeletePersonPropertyRelation;

/// <summary>
/// Validator for DeletePersonPropertyRelationCommand
/// </summary>
public class DeletePersonPropertyRelationCommandValidator : AbstractValidator<DeletePersonPropertyRelationCommand>
{
    public DeletePersonPropertyRelationCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage("Survey ID is required");

        RuleFor(x => x.RelationId)
            .NotEmpty().WithMessage("Relation ID is required");
    }
}
