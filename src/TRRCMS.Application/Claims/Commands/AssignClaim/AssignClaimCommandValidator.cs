using FluentValidation;

namespace TRRCMS.Application.Claims.Commands.AssignClaim;

/// <summary>
/// Validator for AssignClaimCommand
/// </summary>
public class AssignClaimCommandValidator : AbstractValidator<AssignClaimCommand>
{
    public AssignClaimCommandValidator()
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty().WithMessage("Claim ID is required");

        RuleFor(x => x.AssignToUserId)
            .NotEmpty().WithMessage("Assignee user ID is required");

        RuleFor(x => x.TargetCompletionDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.TargetCompletionDate.HasValue)
            .WithMessage("Target completion date cannot be in the past");

        RuleFor(x => x.ModifiedByUserId)
            .NotEmpty().WithMessage("Modified by user ID is required");

        // Cannot assign to self
        RuleFor(x => x)
            .Must(x => x.AssignToUserId != x.ModifiedByUserId)
            .WithMessage("Cannot assign a claim to yourself");
    }
}
