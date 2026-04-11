using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Import.Commands.QuarantinePackage;

public class QuarantinePackageCommandValidator : LocalizedValidator<QuarantinePackageCommand>
{
    public QuarantinePackageCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty().WithMessage(L("ImportPackageId_Required"));

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(L("QuarantineReason_Required"))
            .MaximumLength(1000).WithMessage(L("QuarantineReason_MaxLength1000"));
    }
}
