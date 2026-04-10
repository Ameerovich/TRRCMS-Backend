using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Import.Commands.ApproveForCommit;

/// <summary>
/// Validator for ApproveForCommitCommand.
/// Ensures the command has a valid package ID and consistent approval mode.
/// </summary>
public class ApproveForCommitCommandValidator : LocalizedValidator<ApproveForCommitCommand>
{
    public ApproveForCommitCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage(L("ImportPackageId_Required"));

        // When not approving all, at least one specific record must be provided
        When(x => !x.ApproveAllValid, () =>
        {
            RuleFor(x => x.StagingRecordIds)
                .NotNull()
                .WithMessage(L("StagingRecordIds_Required"))
                .Must(ids => ids != null && ids.Count > 0)
                .WithMessage(L("StagingRecordIds_AtLeastOne"));

            RuleForEach(x => x.StagingRecordIds!)
                .NotEmpty()
                .WithMessage(L("StagingRecordId_ValidGuid"));
        });
    }
}
