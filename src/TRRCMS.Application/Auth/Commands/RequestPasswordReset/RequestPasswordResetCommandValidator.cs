using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;

namespace TRRCMS.Application.Auth.Commands.RequestPasswordReset;

public class RequestPasswordResetCommandValidator : LocalizedValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(L("Username_Required"))
            .MaximumLength(50).WithMessage(L("Username_MaxLength50"));

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage(L("Note_MaxLength500"));
    }
}
