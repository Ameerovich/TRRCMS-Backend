using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Evidences.Commands.CreateEvidence;

/// <summary>
/// Validator for CreateEvidenceCommand
/// Validates evidence creation with file metadata and linked entity rules
/// </summary>
public class CreateEvidenceCommandValidator : LocalizedValidator<CreateEvidenceCommand>
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

    public CreateEvidenceCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        // ==================== REQUIRED FIELDS ====================

        RuleFor(x => x.EvidenceType)
            .IsInEnum().WithMessage(L("EvidenceType_NotRecognized"))
            .Must(v => vocabService.IsValidCode("evidence_type", (int)v))
            .WithMessage(L("EvidenceType_NotRecognized"));

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(L("Description_Required"))
            .MaximumLength(500).WithMessage(L("Description_MaxLength500"));

        RuleFor(x => x.OriginalFileName)
            .NotEmpty().WithMessage(L("FileName_Required"))
            .MaximumLength(255).WithMessage(L("FileName_MaxLength255"));

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage(L("FilePath_Required"))
            .MaximumLength(1000).WithMessage(L("FilePath_MaxLength1000"));

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage(L("File_SizeGreaterThanZero"))
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage(L("File_SizeExceedsMax", MaxFileSizeBytes / (1024 * 1024)));

        RuleFor(x => x.MimeType)
            .NotEmpty().WithMessage(L("MimeType_Required"))
            .Must(mime => AllowedMimeTypes.Contains(mime.ToLowerInvariant()))
            .WithMessage(L("File_TypeNotAllowed_Evidence"));

        // ==================== HASH VALIDATION ====================

        RuleFor(x => x.FileHash)
            .Matches(@"^[a-fA-F0-9]{64}$")
            .When(x => !string.IsNullOrWhiteSpace(x.FileHash))
            .WithMessage(L("FileHash_InvalidSha256"));

        // ==================== DATE VALIDATIONS ====================

        RuleFor(x => x.DocumentIssuedDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.DocumentIssuedDate.HasValue)
            .WithMessage(L("IssueDate_NotFuture"));

        RuleFor(x => x.DocumentExpiryDate)
            .GreaterThan(x => x.DocumentIssuedDate)
            .When(x => x.DocumentIssuedDate.HasValue && x.DocumentExpiryDate.HasValue)
            .WithMessage(L("ExpiryDate_AfterIssue"));

        // ==================== OPTIONAL TEXT FIELDS ====================

        RuleFor(x => x.IssuingAuthority)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.IssuingAuthority))
            .WithMessage(L("IssuingAuthority_MaxLength200"));

        RuleFor(x => x.DocumentReferenceNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentReferenceNumber))
            .WithMessage(L("DocumentRef_MaxLength100"));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage(L("Notes_MaxLength1000"));
    }
}
