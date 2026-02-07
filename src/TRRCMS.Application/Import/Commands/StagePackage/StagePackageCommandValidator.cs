using FluentValidation;

namespace TRRCMS.Application.Import.Commands.StagePackage;

/// <summary>
/// Validator for StagePackageCommand.
/// The handler performs additional business rule checks (package exists, correct status).
/// </summary>
public class StagePackageCommandValidator : AbstractValidator<StagePackageCommand>
{
    public StagePackageCommandValidator()
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage("Import package ID is required");
    }
}
