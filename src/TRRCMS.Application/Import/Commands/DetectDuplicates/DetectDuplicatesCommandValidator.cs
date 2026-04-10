using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Import.Commands.DetectDuplicates;

/// <summary>
/// Validator for DetectDuplicatesCommand.
/// Business rule checks (package exists, correct status) are in the handler.
/// </summary>
public class DetectDuplicatesCommandValidator : LocalizedValidator<DetectDuplicatesCommand>
{
    public DetectDuplicatesCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage(L("ImportPackageId_Required"));
    }
}
