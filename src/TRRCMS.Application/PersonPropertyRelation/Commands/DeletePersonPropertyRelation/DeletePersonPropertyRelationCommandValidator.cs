using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.DeletePersonPropertyRelation;

public class DeletePersonPropertyRelationCommandValidator : LocalizedValidator<DeletePersonPropertyRelationCommand>
{
    public DeletePersonPropertyRelationCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.RelationId)
            .NotEmpty()
            .WithMessage(L("RelationId_Required"));
    }
}
