using FluentValidation;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.DeletePersonPropertyRelation;

public class DeletePersonPropertyRelationCommandValidator : AbstractValidator<DeletePersonPropertyRelationCommand>
{
    public DeletePersonPropertyRelationCommandValidator()
    {
        RuleFor(x => x.RelationId)
            .NotEmpty()
            .WithMessage("Relation ID is required");
    }
}
