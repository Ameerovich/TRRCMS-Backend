using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Cases.Commands.ReopenCase;

public class ReopenCaseCommandValidator : LocalizedValidator<ReopenCaseCommand>
{
    public ReopenCaseCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.CaseId)
            .NotEmpty().WithMessage(L("CaseId_Required"));
    }
}
