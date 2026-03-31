using FluentValidation;

namespace TRRCMS.Application.Landmarks.Commands.DeleteLandmark;

public class DeleteLandmarkCommandValidator : AbstractValidator<DeleteLandmarkCommand>
{
    public DeleteLandmarkCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Landmark ID is required.");
    }
}
