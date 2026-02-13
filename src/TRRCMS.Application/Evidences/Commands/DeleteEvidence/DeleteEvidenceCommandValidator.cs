using FluentValidation;

namespace TRRCMS.Application.Evidences.Commands.DeleteEvidence;

public class DeleteEvidenceCommandValidator : AbstractValidator<DeleteEvidenceCommand>
{
    public DeleteEvidenceCommandValidator()
    {
        RuleFor(x => x.EvidenceId)
            .NotEmpty()
            .WithMessage("Evidence ID is required");
    }
}
