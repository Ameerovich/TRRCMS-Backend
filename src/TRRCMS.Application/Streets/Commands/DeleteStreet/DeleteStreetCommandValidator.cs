using FluentValidation;

namespace TRRCMS.Application.Streets.Commands.DeleteStreet;

public class DeleteStreetCommandValidator : AbstractValidator<DeleteStreetCommand>
{
    public DeleteStreetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Street ID is required.");
    }
}
