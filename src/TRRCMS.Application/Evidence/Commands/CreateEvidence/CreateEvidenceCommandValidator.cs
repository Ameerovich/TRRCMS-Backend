using FluentValidation;

namespace TRRCMS.Application.Evidences.Commands.CreateEvidence;

/// <summary>
/// Validator for CreateEvidenceCommand
/// Validates evidence creation with file metadata and linked entity rules
/// </summary>
public class CreateEvidenceCommandValidator : AbstractValidator<CreateEvidenceCommand>
{
    /// <summary>
    /// Maximum file size: 25 MB
    /// </summary>
    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private static readonly string[] AllowedMimeTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/tiff",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public CreateEvidenceCommandValidator()
    {
        // ==================== REQUIRED FIELDS ====================

        RuleFor(x => x.EvidenceType)
            .IsInEnum().WithMessage("Invalid evidence type");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.OriginalFileName)
            .NotEmpty().WithMessage("Original file name is required")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required")
            .MaximumLength(1000).WithMessage("File path cannot exceed 1000 characters");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File size must be greater than 0")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB");

        RuleFor(x => x.MimeType)
            .NotEmpty().WithMessage("MIME type is required")
            .Must(mime => AllowedMimeTypes.Contains(mime.ToLowerInvariant()))
            .WithMessage("File type not allowed. Accepted types: JPEG, PNG, GIF, WebP, TIFF, PDF, DOC, DOCX");

        // ==================== HASH VALIDATION ====================

        RuleFor(x => x.FileHash)
            .Matches(@"^[a-fA-F0-9]{64}$")
            .When(x => !string.IsNullOrWhiteSpace(x.FileHash))
            .WithMessage("File hash must be a valid SHA-256 hex string (64 characters)");

        // ==================== DATE VALIDATIONS ====================

        RuleFor(x => x.DocumentIssuedDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.DocumentIssuedDate.HasValue)
            .WithMessage("Document issue date cannot be in the future");

        RuleFor(x => x.DocumentExpiryDate)
            .GreaterThan(x => x.DocumentIssuedDate)
            .When(x => x.DocumentIssuedDate.HasValue && x.DocumentExpiryDate.HasValue)
            .WithMessage("Expiry date must be after issue date");

        // ==================== OPTIONAL TEXT FIELDS ====================

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
