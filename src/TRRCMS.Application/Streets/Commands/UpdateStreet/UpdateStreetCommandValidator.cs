using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Streets.Commands.UpdateStreet;

public class UpdateStreetCommandValidator : LocalizedValidator<UpdateStreetCommand>
{
    public UpdateStreetCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(L("StreetId_Required"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(L("StreetName_Required"))
            .MaximumLength(500).WithMessage(L("StreetName_MaxLength500"));
    }
}
