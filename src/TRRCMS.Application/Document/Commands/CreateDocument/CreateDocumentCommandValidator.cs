using FluentValidation;

namespace TRRCMS.Application.Documents.Commands.CreateDocument;

/// <summary>
/// Validator for CreateDocumentCommand
/// Validates document creation with type, dates, and linked entity rules
/// </summary>
public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        // ==================== REQUIRED FIELDS ====================

        RuleFor(x => x.DocumentType)
            .IsInEnum().WithMessage("Invalid document type");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required");

        // ==================== OPTIONAL TEXT FIELDS ====================

        RuleFor(x => x.DocumentNumber)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentNumber))
            .WithMessage("Document number cannot exceed 100 characters");

        RuleFor(x => x.DocumentTitle)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentTitle))
            .WithMessage("Document title cannot exceed 500 characters");

        RuleFor(x => x.IssuingAuthority)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.IssuingAuthority))
            .WithMessage("Issuing authority cannot exceed 200 characters");

        RuleFor(x => x.IssuingPlace)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.IssuingPlace))
            .WithMessage("Issuing place cannot exceed 200 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 2000 characters");

        // ==================== DATE VALIDATIONS ====================

        RuleFor(x => x.IssueDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.IssueDate.HasValue)
            .WithMessage("Issue date cannot be in the future");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.IssueDate)
            .When(x => x.IssueDate.HasValue && x.ExpiryDate.HasValue)
            .WithMessage("Expiry date must be after issue date");

        // ==================== HASH VALIDATION ====================

        RuleFor(x => x.DocumentHash)
            .Matches(@"^[a-fA-F0-9]{64}$")
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentHash))
            .WithMessage("Document hash must be a valid SHA-256 hex string (64 characters)");
    }
}
