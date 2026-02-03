using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.DeleteEvidence;

/// <summary>
/// Validator for DeleteEvidenceCommand
/// </summary>
public class DeleteEvidenceCommandValidator : AbstractValidator<DeleteEvidenceCommand>
{
    public DeleteEvidenceCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage("Survey ID is required");

        RuleFor(x => x.EvidenceId)
            .NotEmpty().WithMessage("Evidence ID is required");
    }
}
