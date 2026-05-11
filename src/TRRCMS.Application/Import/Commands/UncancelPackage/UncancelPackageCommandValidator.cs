using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application;
using TRRCMS.Application.Common.Localization;

namespace TRRCMS.Application.Import.Commands.UncancelPackage;

public class UncancelPackageCommandValidator : LocalizedValidator<UncancelPackageCommand>
{
    public UncancelPackageCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty().WithMessage(L("ImportPackageId_Required"));

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => x.Reason != null)
            .WithMessage(L("CancellationReason_MaxLength1000"));
    }
}
