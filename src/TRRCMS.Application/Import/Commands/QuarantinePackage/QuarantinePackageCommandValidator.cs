using FluentValidation;

namespace TRRCMS.Application.Import.Commands.QuarantinePackage;

public class QuarantinePackageCommandValidator : AbstractValidator<QuarantinePackageCommand>
{
    public QuarantinePackageCommandValidator()
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty().WithMessage("ImportPackageId is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Quarantine reason is required.")
            .MaximumLength(1000).WithMessage("Quarantine reason must not exceed 1000 characters.");
    }
}
