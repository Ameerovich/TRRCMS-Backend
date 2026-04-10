using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Surveys.Commands.DeleteEvidence;

/// <summary>
/// Validator for DeleteEvidenceCommand
/// </summary>
public class DeleteEvidenceCommandValidator : LocalizedValidator<DeleteEvidenceCommand>
{
    public DeleteEvidenceCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.EvidenceId)
            .NotEmpty().WithMessage(L("EvidenceId_Required"));
    }
}
