using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UploadPropertyPhoto;

/// <summary>
/// Validator for UploadPropertyPhotoCommand
/// Enhanced with file size limits and allowed image MIME types
/// </summary>
public class UploadPropertyPhotoCommandValidator : AbstractValidator<UploadPropertyPhotoCommand>
{
    /// <summary>
    /// Maximum photo file size: 10 MB
    /// </summary>
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly string[] AllowedImageMimeTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/tiff"
    };

    public UploadPropertyPhotoCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        // File size validation
        RuleFor(x => x.File)
            .Must(file => file.Length <= MaxFileSizeBytes)
            .When(x => x.File != null)
            .WithMessage($"Photo file size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB");

        RuleFor(x => x.File)
            .Must(file => file.Length > 0)
            .When(x => x.File != null)
            .WithMessage("Photo file cannot be empty");

        // MIME type validation - photos only
        RuleFor(x => x.File)
            .Must(file => AllowedImageMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            .When(x => x.File != null)
            .WithMessage("Only image files are allowed (JPEG, PNG, GIF, WebP, TIFF)");

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
