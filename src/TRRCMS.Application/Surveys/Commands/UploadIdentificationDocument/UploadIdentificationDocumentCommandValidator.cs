using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UploadIdentificationDocument;

/// <summary>
/// Validator for UploadIdentificationDocumentCommand
/// Enhanced with file size limits and allowed MIME types for ID documents
/// </summary>
public class UploadIdentificationDocumentCommandValidator : AbstractValidator<UploadIdentificationDocumentCommand>
{
    /// <summary>
    /// Maximum document file size: 15 MB
    /// </summary>
    private const long MaxFileSizeBytes = 15 * 1024 * 1024;

    private static readonly string[] AllowedMimeTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/tiff",
        "application/pdf"
    };

    public UploadIdentificationDocumentCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.PersonId)
            .NotEmpty()
            .WithMessage("Person ID is required");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        // File size validation
        RuleFor(x => x.File)
            .Must(file => file.Length <= MaxFileSizeBytes)
            .When(x => x.File != null)
            .WithMessage($"Document file size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB");

        RuleFor(x => x.File)
            .Must(file => file.Length > 0)
            .When(x => x.File != null)
            .WithMessage("Document file cannot be empty");

        // MIME type validation - images and PDFs for ID documents
        RuleFor(x => x.File)
            .Must(file => AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            .When(x => x.File != null)
            .WithMessage("Only image files and PDFs are allowed (JPEG, PNG, GIF, WebP, TIFF, PDF)");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.DocumentExpiryDate)
            .GreaterThan(x => x.DocumentIssuedDate)
            .When(x => x.DocumentIssuedDate.HasValue && x.DocumentExpiryDate.HasValue)
            .WithMessage("Expiry date must be after issue date");

        RuleFor(x => x.DocumentIssuedDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.DocumentIssuedDate.HasValue)
            .WithMessage("Document issue date cannot be in the future");

        RuleFor(x => x.IssuingAuthority)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.IssuingAuthority))
            .WithMessage("Issuing authority cannot exceed 200 characters");

        RuleFor(x => x.DocumentReferenceNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentReferenceNumber))
            .WithMessage("Document reference number cannot exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 1000 characters");
    }
}
