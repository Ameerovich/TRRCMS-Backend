using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Import.Commands.CancelPackage;

public class CancelPackageCommandValidator : LocalizedValidator<CancelPackageCommand>
{
    public CancelPackageCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty().WithMessage(L("ImportPackageId_Required"));

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(L("CancellationReason_Required_Import"))
            .MaximumLength(1000).WithMessage(L("CancellationReason_MaxLength1000"));
    }
}
