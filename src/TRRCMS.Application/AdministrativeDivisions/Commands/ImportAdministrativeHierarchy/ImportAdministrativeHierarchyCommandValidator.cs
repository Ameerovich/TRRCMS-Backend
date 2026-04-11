using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.AdministrativeDivisions.Commands.ImportAdministrativeHierarchy;

public class ImportAdministrativeHierarchyCommandValidator : LocalizedValidator<ImportAdministrativeHierarchyCommand>
{
    public ImportAdministrativeHierarchyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.JsonContent)
            .NotEmpty().WithMessage(L("JsonContent_Required"))
            .Must(json => json.TrimStart().StartsWith("[") || json.TrimStart().StartsWith("{"))
            .WithMessage(L("JsonContent_InvalidFormat"));

        RuleFor(x => x.ImportedByUserId)
            .NotEmpty().WithMessage(L("ImportedByUserId_Required"));
    }
}
