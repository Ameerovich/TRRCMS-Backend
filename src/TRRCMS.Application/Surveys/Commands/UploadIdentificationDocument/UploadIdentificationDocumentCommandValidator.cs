using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UploadIdentificationDocument;

/// <summary>
/// Validator for UploadIdentificationDocumentCommand
/// </summary>
public class UploadIdentificationDocumentCommandValidator : AbstractValidator<UploadIdentificationDocumentCommand>
{
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

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.DocumentExpiryDate)
            .GreaterThan(x => x.DocumentIssuedDate)
            .When(x => x.DocumentIssuedDate.HasValue && x.DocumentExpiryDate.HasValue)
            .WithMessage("Expiry date must be after issue date");

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