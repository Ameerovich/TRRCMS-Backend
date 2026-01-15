using FluentValidation;

namespace TRRCMS.Application.Claims.Commands.UpdateClaim;

public class UpdateClaimCommandValidator : AbstractValidator<UpdateClaimCommand>
{
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

        RuleFor(x => x.ClaimType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ClaimType))
            .WithMessage("Claim type must not exceed 100 characters");

        RuleFor(x => x.TenureContractDetails)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.TenureContractDetails))
            .WithMessage("Tenure contract details must not exceed 1000 characters");

        RuleFor(x => x.ProcessingNotes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.ProcessingNotes))
            .WithMessage("Processing notes must not exceed 2000 characters");

        RuleFor(x => x.PublicRemarks)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.PublicRemarks))
            .WithMessage("Public remarks must not exceed 2000 characters");
    }
}