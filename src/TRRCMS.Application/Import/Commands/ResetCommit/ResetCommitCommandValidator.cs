using FluentValidation;

namespace TRRCMS.Application.Import.Commands.ResetCommit;

public class ResetCommitCommandValidator : AbstractValidator<ResetCommitCommand>
{
    public ResetCommitCommandValidator()
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty().WithMessage("Import package ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reset reason is required for audit trail.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
