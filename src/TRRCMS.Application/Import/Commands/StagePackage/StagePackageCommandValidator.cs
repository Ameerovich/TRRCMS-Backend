using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Import.Commands.StagePackage;

/// <summary>
/// Validator for StagePackageCommand.
/// The handler performs additional business rule checks (package exists, correct status).
/// </summary>
public class StagePackageCommandValidator : LocalizedValidator<StagePackageCommand>
{
    public StagePackageCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage(L("ImportPackageId_Required"));
    }
}
