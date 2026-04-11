using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Claims.Commands.UpdateClaim;

public class UpdateClaimCommandValidator : LocalizedValidator<UpdateClaimCommand>
{
    public UpdateClaimCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage(L("ClaimId_Required"));

        RuleFor(x => x.ReasonForModification)
            .NotEmpty()
            .WithMessage(L("ClaimReason_Required"))
            .MinimumLength(10)
            .WithMessage(L("ClaimReason_MinLength10"))
            .MaximumLength(500)
            .WithMessage(L("ClaimReason_MaxLength500"));

        // ---- Relation fields ----

        RuleFor(x => x.RelationType)
            .Must(v => vocabService.IsValidCode("relation_type", v!.Value))
            .When(x => x.RelationType.HasValue)
            .WithMessage(L("RelationType_InvalidClaim"));

        RuleFor(x => x.OccupancyType)
            .Must(v => vocabService.IsValidCode("occupancy_type", v!.Value))
            .When(x => x.OccupancyType.HasValue)
            .WithMessage(L("OccupancyType_Invalid"));

        RuleFor(x => x.OwnershipShare)
            .GreaterThan(0)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage(L("OwnershipShare_GreaterThanZero"))
            .LessThanOrEqualTo(2400)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage(L("OwnershipShare_Max2400"));

        RuleFor(x => x.ContractDetails)
            .MaximumLength(1000)
            .When(x => x.ContractDetails != null)
            .WithMessage(L("ContractDetails_MaxLength1000"));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes != null)
            .WithMessage(L("Notes_MaxLength2000"));

        // ---- Claim-level fields ----

        RuleFor(x => x.TenureContractType)
            .Must(v => vocabService.IsValidCode("tenure_contract_type", v!.Value))
            .When(x => x.TenureContractType.HasValue)
            .WithMessage(L("TenureContractType_Invalid"));

        RuleFor(x => x.TenureContractDetails)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.TenureContractDetails))
            .WithMessage(L("TenureContractDetails_MaxLength1000"));

        // ---- Evidence: new evidence items ----

        RuleForEach(x => x.NewEvidence)
            .SetValidator(new CreateAndLinkEvidenceDtoValidator(localizer, vocabService))
            .When(x => x.NewEvidence != null && x.NewEvidence.Count > 0);

        // ---- Evidence: link/unlink IDs ----

        RuleForEach(x => x.LinkExistingEvidenceIds)
            .NotEmpty()
            .WithMessage(L("EvidenceIdNotEmpty"))
            .When(x => x.LinkExistingEvidenceIds != null);

        RuleForEach(x => x.UnlinkEvidenceRelationIds)
            .NotEmpty()
            .WithMessage(L("EvidenceRelationIdNotEmpty"))
            .When(x => x.UnlinkEvidenceRelationIds != null);
    }
}

public class CreateAndLinkEvidenceDtoValidator : LocalizedValidator<CreateAndLinkEvidenceDto>
{
    public CreateAndLinkEvidenceDtoValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.EvidenceType)
            .Must(v => vocabService.IsValidCode("evidence_type", v))
            .WithMessage(L("EvidenceType_Invalid"));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(L("EvidenceDescription_Required"))
            .MaximumLength(2000);

        RuleFor(x => x.OriginalFileName)
            .NotEmpty()
            .WithMessage(L("FileName_Required"))
            .MaximumLength(500);

        RuleFor(x => x.FilePath)
            .NotEmpty()
            .WithMessage(L("FilePath_Required"));

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage(L("File_SizeGreaterThanZero"));

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .WithMessage(L("MimeType_Required"))
            .MaximumLength(100);
    }
}
