using FluentValidation;
using TRRCMS.Application.Claims.Dtos;

namespace TRRCMS.Application.Claims.Commands.UpdateClaim;

public class UpdateClaimCommandValidator : AbstractValidator<UpdateClaimCommand>
{
    private static readonly int[] ValidRelationTypes = { 1, 2, 3, 4, 5, 99 };
    private static readonly int[] ValidOccupancyTypes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 99 };
    private static readonly int[] ValidTenureContractTypes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 99 };
    private static readonly int[] ValidEvidenceTypes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 99 };

    public UpdateClaimCommandValidator()
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
            .Must(v => ValidRelationTypes.Contains(v!.Value))
            .When(x => x.RelationType.HasValue)
            .WithMessage("Invalid relation type. Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99.");

        RuleFor(x => x.OccupancyType)
            .Must(v => ValidOccupancyTypes.Contains(v!.Value))
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
            .Must(v => ValidTenureContractTypes.Contains(v!.Value))
            .When(x => x.TenureContractType.HasValue)
            .WithMessage("Invalid tenure contract type.");

        RuleFor(x => x.TenureContractDetails)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.TenureContractDetails))
            .WithMessage("Tenure contract details must not exceed 1000 characters");

        // ---- Evidence: new evidence items ----

        RuleForEach(x => x.NewEvidence)
            .SetValidator(new CreateAndLinkEvidenceDtoValidator())
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
    private static readonly int[] ValidEvidenceTypes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 99 };

    public CreateAndLinkEvidenceDtoValidator()
    {
        RuleFor(x => x.EvidenceType)
            .Must(v => ValidEvidenceTypes.Contains(v))
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
