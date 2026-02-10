using FluentValidation;

namespace TRRCMS.Application.Import.Commands.CancelPackage;

public class CancelPackageCommandValidator : AbstractValidator<CancelPackageCommand>
{
    public CancelPackageCommandValidator()
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty().WithMessage("ImportPackageId is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MaximumLength(1000).WithMessage("Cancellation reason must not exceed 1000 characters.");
    }
}
