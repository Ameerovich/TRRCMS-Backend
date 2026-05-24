using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Surveys.Commands.UnlinkEvidenceFromRelation;

/// <summary>
/// Validator for UnlinkEvidenceFromRelationCommand.
/// </summary>
public class UnlinkEvidenceFromRelationCommandValidator : LocalizedValidator<UnlinkEvidenceFromRelationCommand>
{
    public UnlinkEvidenceFromRelationCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
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

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage(L("LinkReason_MaxLength500"));
    }
}
