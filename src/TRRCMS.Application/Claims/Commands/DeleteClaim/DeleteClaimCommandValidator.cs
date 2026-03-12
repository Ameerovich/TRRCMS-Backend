using FluentValidation;

namespace TRRCMS.Application.Claims.Commands.DeleteClaim;

public class DeleteClaimCommandValidator : AbstractValidator<DeleteClaimCommand>
{
    public DeleteClaimCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage("Claim ID is required");
    }
}
