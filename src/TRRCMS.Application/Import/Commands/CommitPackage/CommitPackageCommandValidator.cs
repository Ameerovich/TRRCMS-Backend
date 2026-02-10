using FluentValidation;

namespace TRRCMS.Application.Import.Commands.CommitPackage;

/// <summary>
/// Validator for CommitPackageCommand.
/// </summary>
public class CommitPackageCommandValidator : AbstractValidator<CommitPackageCommand>
{
    public CommitPackageCommandValidator()
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage("Import package ID is required.");
    }
}
