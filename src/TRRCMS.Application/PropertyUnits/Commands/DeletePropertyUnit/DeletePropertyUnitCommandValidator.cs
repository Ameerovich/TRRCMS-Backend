using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.PropertyUnits.Commands.DeletePropertyUnit;

public class DeletePropertyUnitCommandValidator : LocalizedValidator<DeletePropertyUnitCommand>
{
    public DeletePropertyUnitCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage(L("PropertyUnitId_Required"));
    }
}
