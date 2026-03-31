using FluentValidation;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Commands.UpdateClaim;

public class UpdateClaimCommandValidator : AbstractValidator<UpdateClaimCommand>
{
    public UpdateClaimCommandValidator(IVocabularyValidationService vocabService)
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage("Claim ID is required");

        RuleFor(x => x.ReasonForModification)
            .NotEmpty()
            .WithMessage("Reason for modification is required for audit purposes")
            .MinimumLength(10)
            .WithMessage("Reason must be at least 10 characters")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters");

        // ---- Relation fields ----

        RuleFor(x => x.RelationType)
            .Must(v => vocabService.IsValidCode("relation_type", v!.Value))
            .When(x => x.RelationType.HasValue)
            .WithMessage("Invalid relation type. Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99.");

        RuleFor(x => x.OccupancyType)
            .Must(v => vocabService.IsValidCode("occupancy_type", v!.Value))
            .When(x => x.OccupancyType.HasValue)
            .WithMessage("Invalid occupancy type.");

        RuleFor(x => x.OwnershipShare)
            .GreaterThan(0)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage("Ownership share must be greater than 0")
            .LessThanOrEqualTo(2400)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage("Ownership share must not exceed 2400");

        RuleFor(x => x.ContractDetails)
            .MaximumLength(1000)
            .When(x => x.ContractDetails != null)
            .WithMessage("Contract details must not exceed 1000 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes != null)
            .WithMessage("Notes must not exceed 2000 characters");

        // ---- Claim-level fields ----

        RuleFor(x => x.TenureContractType)
            .Must(v => vocabService.IsValidCode("tenure_contract_type", v!.Value))
            .When(x => x.TenureContractType.HasValue)
            .WithMessage("Invalid tenure contract type.");

        RuleFor(x => x.TenureContractDetails)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.TenureContractDetails))
            .WithMessage("Tenure contract details must not exceed 1000 characters");

        // ---- Evidence: new evidence items ----

        RuleForEach(x => x.NewEvidence)
            .SetValidator(new CreateAndLinkEvidenceDtoValidator(vocabService))
            .When(x => x.NewEvidence != null && x.NewEvidence.Count > 0);

        // ---- Evidence: link/unlink IDs ----

        RuleForEach(x => x.LinkExistingEvidenceIds)
            .NotEmpty()
            .WithMessage("Evidence ID must not be empty")
            .When(x => x.LinkExistingEvidenceIds != null);

        RuleForEach(x => x.UnlinkEvidenceRelationIds)
            .NotEmpty()
            .WithMessage("EvidenceRelation ID must not be empty")
            .When(x => x.UnlinkEvidenceRelationIds != null);
    }
}

public class CreateAndLinkEvidenceDtoValidator : AbstractValidator<CreateAndLinkEvidenceDto>
{
    public CreateAndLinkEvidenceDtoValidator(IVocabularyValidationService vocabService)
    {
        RuleFor(x => x.EvidenceType)
            .Must(v => vocabService.IsValidCode("evidence_type", v))
            .WithMessage("Invalid evidence type.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Evidence description is required")
            .MaximumLength(2000);

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .WithMessage("Original file name is required")
            .MaximumLength(500);

        RuleFor(x => x.FilePath)
            .NotEmpty()
            .WithMessage("File path is required");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("File size must be greater than 0");

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .WithMessage("MIME type is required")
            .MaximumLength(100);
    }
}
