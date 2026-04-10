using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.UploadPropertyPhoto;

/// <summary>
/// Validator for UploadPropertyPhotoCommand
/// Enhanced with file size limits and allowed image MIME types
/// </summary>
public class UploadPropertyPhotoCommandValidator : LocalizedValidator<UploadPropertyPhotoCommand>
{
    /// <summary>
    /// Maximum photo file size: 10 MB
    /// </summary>
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly string[] AllowedImageMimeTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/tiff"
    };

    public UploadPropertyPhotoCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage(L("File_Required"));

        // File size validation
        RuleFor(x => x.File)
            .Must(file => file.Length <= MaxFileSizeBytes)
            .When(x => x.File != null)
            .WithMessage(L("PhotoFile_SizeExceedsMax", MaxFileSizeBytes / (1024 * 1024)));

        RuleFor(x => x.File)
            .Must(file => file.Length > 0)
            .When(x => x.File != null)
            .WithMessage(L("PhotoFile_Empty"));

        // MIME type validation - photos only
        RuleFor(x => x.File)
            .Must(file => AllowedImageMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            .When(x => x.File != null)
            .WithMessage(L("File_TypeNotAllowed_ImageOnly"));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(L("Description_Required"))
            .MaximumLength(500)
            .WithMessage(L("Description_MaxLength500"));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage(L("Notes_MaxLength1000"));
    }
}
