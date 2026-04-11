using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Surveys.Commands.DeletePersonPropertyRelation;

/// <summary>
/// Validator for DeletePersonPropertyRelationCommand
/// </summary>
public class DeletePersonPropertyRelationCommandValidator : LocalizedValidator<DeletePersonPropertyRelationCommand>
{
    public DeletePersonPropertyRelationCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.RelationId)
            .NotEmpty().WithMessage(L("RelationId_Required"));
    }
}
