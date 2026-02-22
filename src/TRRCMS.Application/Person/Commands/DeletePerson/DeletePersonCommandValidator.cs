using FluentValidation;

namespace TRRCMS.Application.Persons.Commands.DeletePerson;

public class DeletePersonCommandValidator : AbstractValidator<DeletePersonCommand>
{
    public DeletePersonCommandValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty()
            .WithMessage("Person ID is required");
    }
}
