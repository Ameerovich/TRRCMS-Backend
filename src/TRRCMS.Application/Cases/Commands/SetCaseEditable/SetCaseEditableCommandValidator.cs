using FluentValidation;

namespace TRRCMS.Application.Cases.Commands.SetCaseEditable;

public class SetCaseEditableCommandValidator : AbstractValidator<SetCaseEditableCommand>
{
    public SetCaseEditableCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .NotEmpty().WithMessage("Case ID is required.");
    }
}
