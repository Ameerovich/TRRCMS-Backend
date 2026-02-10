using FluentValidation;

namespace TRRCMS.Application.Import.Commands.ApproveForCommit;

/// <summary>
/// Validator for ApproveForCommitCommand.
/// Ensures the command has a valid package ID and consistent approval mode.
/// </summary>
public class ApproveForCommitCommandValidator : AbstractValidator<ApproveForCommitCommand>
{
    public ApproveForCommitCommandValidator()
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage("Import package ID is required.");

        // When not approving all, at least one specific record must be provided
        When(x => !x.ApproveAllValid, () =>
        {
            RuleFor(x => x.StagingRecordIds)
                .NotNull()
                .WithMessage("Staging record IDs are required when not approving all valid records.")
                .Must(ids => ids != null && ids.Count > 0)
                .WithMessage("At least one staging record ID must be specified.");

            RuleForEach(x => x.StagingRecordIds!)
                .NotEmpty()
                .WithMessage("Each staging record ID must be a valid GUID.");
        });
    }
}
