using FluentValidation;

namespace TRRCMS.Application.Cases.Commands.ReopenCase;

public class ReopenCaseCommandValidator : AbstractValidator<ReopenCaseCommand>
{
    public ReopenCaseCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .NotEmpty().WithMessage("Case ID is required.");
    }
}
