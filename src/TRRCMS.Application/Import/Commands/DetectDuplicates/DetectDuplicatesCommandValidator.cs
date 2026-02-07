using FluentValidation;

namespace TRRCMS.Application.Import.Commands.DetectDuplicates;

/// <summary>
/// Validator for DetectDuplicatesCommand.
/// Business rule checks (package exists, correct status) are in the handler.
/// </summary>
public class DetectDuplicatesCommandValidator : AbstractValidator<DetectDuplicatesCommand>
{
    public DetectDuplicatesCommandValidator()
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage("Import package ID is required");
    }
}
