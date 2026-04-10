using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Conflicts.Commands.EscalateConflict;

/// <summary>
/// Validator for <see cref="EscalateConflictCommand"/>.
/// </summary>
public class EscalateConflictCommandValidator : LocalizedValidator<EscalateConflictCommand>
{
    public EscalateConflictCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ConflictId)
            .NotEmpty()
            .WithMessage(L("ConflictId_Required"));

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(L("EscalationReason_Required"))
            .MaximumLength(2000)
            .WithMessage(L("EscalationReason_MaxLength2000"));
    }
}
