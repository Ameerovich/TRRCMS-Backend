using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Cases.Commands.SetCaseEditable;

public class SetCaseEditableCommandValidator : LocalizedValidator<SetCaseEditableCommand>
{
    public SetCaseEditableCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.CaseId)
            .NotEmpty().WithMessage(L("CaseId_Required"));
    }
}
