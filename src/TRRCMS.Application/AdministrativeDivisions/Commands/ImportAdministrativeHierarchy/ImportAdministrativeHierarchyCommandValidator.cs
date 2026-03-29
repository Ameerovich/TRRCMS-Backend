using FluentValidation;

namespace TRRCMS.Application.AdministrativeDivisions.Commands.ImportAdministrativeHierarchy;

public class ImportAdministrativeHierarchyCommandValidator : AbstractValidator<ImportAdministrativeHierarchyCommand>
{
    public ImportAdministrativeHierarchyCommandValidator()
    {
        RuleFor(x => x.JsonContent)
            .NotEmpty().WithMessage("JSON content is required.")
            .Must(json => json.TrimStart().StartsWith("[") || json.TrimStart().StartsWith("{"))
            .WithMessage("Content must be valid JSON (must start with [ or {).");

        RuleFor(x => x.ImportedByUserId)
            .NotEmpty().WithMessage("Imported by user ID is required.");
    }
}
