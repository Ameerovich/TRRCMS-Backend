using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UploadPropertyPhoto;

/// <summary>
/// Validator for UploadPropertyPhotoCommand
/// </summary>
public class UploadPropertyPhotoCommandValidator : AbstractValidator<UploadPropertyPhotoCommand>
{
    public UploadPropertyPhotoCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters");
    }
}