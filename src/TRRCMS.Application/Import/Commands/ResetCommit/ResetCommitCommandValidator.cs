using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Import.Commands.ResetCommit;

public class ResetCommitCommandValidator : LocalizedValidator<ResetCommitCommand>
{
    public ResetCommitCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty().WithMessage(L("ImportPackageId_Required"));

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(L("ResetReason_Required"))
            .MaximumLength(500).WithMessage(L("ResetReason_MaxLength500"));
    }
}
