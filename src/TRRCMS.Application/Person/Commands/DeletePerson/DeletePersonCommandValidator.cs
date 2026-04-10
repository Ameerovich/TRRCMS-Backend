using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Persons.Commands.DeletePerson;

public class DeletePersonCommandValidator : LocalizedValidator<DeletePersonCommand>
{
    public DeletePersonCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.PersonId)
            .NotEmpty()
            .WithMessage(L("PersonId_Required"));
    }
}
