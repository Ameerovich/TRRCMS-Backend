using FluentValidation;

namespace TRRCMS.Application.Claims.Commands.VerifyClaim;

/// <summary>
/// Validator for VerifyClaimCommand
/// </summary>
public class VerifyClaimCommandValidator : AbstractValidator<VerifyClaimCommand>
{
    public VerifyClaimCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("Claim ID is required");

        RuleFor(x => x.VerifiedByUserId)
            .NotEmpty().WithMessage("Verified by user ID is required");

        RuleFor(x => x.VerificationNotes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.VerificationNotes))
            .WithMessage("Verification notes cannot exceed 2000 characters");
    }
}
