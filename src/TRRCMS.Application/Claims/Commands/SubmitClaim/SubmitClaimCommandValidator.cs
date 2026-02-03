using FluentValidation;

namespace TRRCMS.Application.Claims.Commands.SubmitClaim;

/// <summary>
/// Validator for SubmitClaimCommand
/// </summary>
public class SubmitClaimCommandValidator : AbstractValidator<SubmitClaimCommand>
{
    public SubmitClaimCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("Claim ID is required");

        RuleFor(x => x.SubmittedByUserId)
            .NotEmpty().WithMessage("Submitted by user ID is required");
    }
}
