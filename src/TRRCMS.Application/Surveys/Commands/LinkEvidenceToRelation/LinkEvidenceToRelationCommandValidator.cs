using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Surveys.Commands.LinkEvidenceToRelation;

/// <summary>
/// Validator for LinkEvidenceToRelationCommand
/// </summary>
public class LinkEvidenceToRelationCommandValidator : LocalizedValidator<LinkEvidenceToRelationCommand>
{
    public LinkEvidenceToRelationCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.EvidenceId)
            .NotEmpty()
            .WithMessage(L("EvidenceId_Required"));

        RuleFor(x => x.PersonPropertyRelationId)
            .NotEmpty()
            .WithMessage(L("PersonPropertyRelationId_Required"));

        RuleFor(x => x.LinkReason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.LinkReason))
            .WithMessage(L("LinkReason_MaxLength500"));
    }
}
