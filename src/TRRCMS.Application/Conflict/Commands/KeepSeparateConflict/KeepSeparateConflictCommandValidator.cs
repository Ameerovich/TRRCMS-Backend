using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Conflicts.Commands.KeepSeparateConflict;

/// <summary>
/// Validator for <see cref="KeepSeparateConflictCommand"/>.
/// Enforces: conflict ID required, mandatory justification reason.
/// </summary>
public class KeepSeparateConflictCommandValidator : LocalizedValidator<KeepSeparateConflictCommand>
{
    public KeepSeparateConflictCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ConflictId)
            .NotEmpty()
            .WithMessage(L("ConflictId_Required"));

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(L("JustificationReason_RequiredKeepSeparate"))
            .MaximumLength(2000)
            .WithMessage(L("Reason_MaxLength2000"));

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .WithMessage(L("Notes_MaxLength4000"));
    }
}
