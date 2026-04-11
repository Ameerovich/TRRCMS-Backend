using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Streets.Commands.DeleteStreet;

public class DeleteStreetCommandValidator : LocalizedValidator<DeleteStreetCommand>
{
    public DeleteStreetCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(L("StreetId_Required"));
    }
}
