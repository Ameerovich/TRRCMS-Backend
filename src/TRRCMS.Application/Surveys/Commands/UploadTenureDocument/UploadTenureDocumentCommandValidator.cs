using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UploadTenureDocument;

/// <summary>
/// Validator for UploadTenureDocumentCommand
/// Enhanced with file size limits and allowed MIME types for tenure/deed documents
/// </summary>
public class UploadTenureDocumentCommandValidator : AbstractValidator<UploadTenureDocumentCommand>
{
    /// <summary>
    /// Maximum document file size: 25 MB (deeds/contracts may be multi-page scans)
    /// </summary>
    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private static readonly string[] AllowedMimeTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/tiff",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public UploadTenureDocumentCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.PersonPropertyRelationId)
            .NotEmpty()
            .WithMessage("Person-property relation ID is required");

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

        // MIME type validation - images, PDFs, and Word docs for tenure documents
        RuleFor(x => x.File)
            .Must(file => AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            .When(x => x.File != null)
            .WithMessage("Only image files, PDFs, and Word documents are allowed (JPEG, PNG, GIF, WebP, TIFF, PDF, DOC, DOCX)");

        // Evidence type validation
        RuleFor(x => x.EvidenceType)
            .IsInEnum()
            .WithMessage("Invalid evidence type");

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
