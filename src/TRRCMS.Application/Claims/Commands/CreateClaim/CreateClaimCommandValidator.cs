using FluentValidation;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Claims.Commands.CreateClaim;

/// <summary>
/// Validator for CreateClaimCommand
/// Comprehensive validation for claim creation per FSD business rules
/// </summary>
public class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    public CreateClaimCommandValidator()
    {
        // ==================== REQUIRED FIELDS ====================

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty().WithMessage("Property unit ID is required");

        RuleFor(x => x.ClaimType)
            .NotEmpty().WithMessage("Claim type is required")
            .MaximumLength(100).WithMessage("Claim type must not exceed 100 characters");

        RuleFor(x => x.ClaimSource)
            .Must(v => Enum.IsDefined(typeof(ClaimSource), v))
            .WithMessage("Invalid claim source");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required");

        // ==================== ENUM VALIDATIONS (int fields) ====================

        RuleFor(x => x.Priority)
            .Must(v => Enum.IsDefined(typeof(CasePriority), v))
            .WithMessage("Invalid case priority value");

        RuleFor(x => x.TenureContractType)
            .Must(v => Enum.IsDefined(typeof(TenureContractType), v!.Value))
            .When(x => x.TenureContractType.HasValue)
            .WithMessage("Invalid tenure contract type");

        // ==================== OWNERSHIP SHARE ====================

        RuleFor(x => x.OwnershipShare)
            .InclusiveBetween(1, 2400)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage("Ownership share must be between 1 and 2400 (fraction out of 2400)");

        // ==================== DATE VALIDATIONS ====================

        RuleFor(x => x.TenureStartDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.TenureStartDate.HasValue)
            .WithMessage("Tenure start date cannot be in the future");

        RuleFor(x => x.TenureEndDate)
            .GreaterThanOrEqualTo(x => x.TenureStartDate)
            .When(x => x.TenureStartDate.HasValue && x.TenureEndDate.HasValue)
            .WithMessage("Tenure end date cannot be before start date");

        RuleFor(x => x.TargetCompletionDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .When(x => x.TargetCompletionDate.HasValue)
            .WithMessage("Target completion date cannot be in the past");

        // ==================== TEXT FIELD LIMITS ====================

        RuleFor(x => x.ClaimDescription)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.ClaimDescription))
            .WithMessage("Claim description must not exceed 2000 characters");

        RuleFor(x => x.LegalBasis)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.LegalBasis))
            .WithMessage("Legal basis must not exceed 1000 characters");

        RuleFor(x => x.SupportingNarrative)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.SupportingNarrative))
            .WithMessage("Supporting narrative must not exceed 4000 characters");

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
