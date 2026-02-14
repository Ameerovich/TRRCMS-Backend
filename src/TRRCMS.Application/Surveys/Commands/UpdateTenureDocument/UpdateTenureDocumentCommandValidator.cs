using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UpdateTenureDocument;

/// <summary>
/// Validator for UpdateTenureDocumentCommand
/// Mirrors UploadTenureDocumentCommandValidator rules (File and EvidenceType are optional for update)
/// </summary>
public class UpdateTenureDocumentCommandValidator : AbstractValidator<UpdateTenureDocumentCommand>
{
    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private static readonly string[] AllowedMimeTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/tiff",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public UpdateTenureDocumentCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.EvidenceId)
            .NotEmpty()
            .WithMessage("Evidence ID is required");

        // File is optional for update (only validate if provided)
        RuleFor(x => x.File)
            .Must(file => file!.Length <= MaxFileSizeBytes)
            .When(x => x.File != null)
            .WithMessage($"Document file size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB");

        RuleFor(x => x.File)
            .Must(file => file!.Length > 0)
            .When(x => x.File != null)
            .WithMessage("Document file cannot be empty");

        RuleFor(x => x.File)
            .Must(file => AllowedMimeTypes.Contains(file!.ContentType.ToLowerInvariant()))
            .When(x => x.File != null)
            .WithMessage("Only image files, PDFs, and Word documents are allowed (JPEG, PNG, GIF, WebP, TIFF, PDF, DOC, DOCX)");

        // Evidence type validation (optional for update)
        RuleFor(x => x.EvidenceType)
            .IsInEnum()
            .When(x => x.EvidenceType.HasValue)
            .WithMessage("Invalid evidence type");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
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
