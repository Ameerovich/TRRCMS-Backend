using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Import.Commands.CommitPackage;

/// <summary>
/// Validator for CommitPackageCommand.
/// </summary>
public class CommitPackageCommandValidator : LocalizedValidator<CommitPackageCommand>
{
    public CommitPackageCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage(L("ImportPackageId_Required"));
    }
}
