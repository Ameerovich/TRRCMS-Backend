using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.UpdateTenureDocument;

/// <summary>
/// Validator for UpdateTenureDocumentCommand
/// Mirrors UploadTenureDocumentCommandValidator rules (File and EvidenceType are optional for update)
/// </summary>
public class UpdateTenureDocumentCommandValidator : LocalizedValidator<UpdateTenureDocumentCommand>
{
    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private static readonly string[] AllowedMimeTypes =
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/tiff",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "audio/mpeg", "audio/wav", "audio/ogg", "audio/mp4", "audio/x-m4a"
    };

    public UpdateTenureDocumentCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.EvidenceId)
            .NotEmpty()
            .WithMessage(L("EvidenceId_Required"));

        // File is optional for update (only validate if provided)
        RuleFor(x => x.File)
            .Must(file => file!.Length <= MaxFileSizeBytes)
            .When(x => x.File != null)
            .WithMessage(L("File_SizeExceedsMax", MaxFileSizeBytes / (1024 * 1024)));

        RuleFor(x => x.File)
            .Must(file => file!.Length > 0)
            .When(x => x.File != null)
            .WithMessage(L("File_Empty"));

        RuleFor(x => x.File)
            .Must(file => AllowedMimeTypes.Contains(file!.ContentType.ToLowerInvariant()))
            .When(x => x.File != null)
            .WithMessage(L("File_TypeNotAllowed_ImagePdf"));

        // Evidence type validation (optional int field for update)
        RuleFor(x => x.EvidenceType)
            .Must(v => vocabService.IsValidCode("evidence_type", v!.Value))
            .When(x => x.EvidenceType.HasValue)
            .WithMessage(L("EvidenceType_NotRecognized"));

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage(L("Description_MaxLength500"));

        RuleFor(x => x.DocumentExpiryDate)
            .GreaterThan(x => x.DocumentIssuedDate)
            .When(x => x.DocumentIssuedDate.HasValue && x.DocumentExpiryDate.HasValue)
            .WithMessage(L("ExpiryDate_AfterIssue"));

        RuleFor(x => x.DocumentIssuedDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.DocumentIssuedDate.HasValue)
            .WithMessage(L("IssueDate_NotFuture"));

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
