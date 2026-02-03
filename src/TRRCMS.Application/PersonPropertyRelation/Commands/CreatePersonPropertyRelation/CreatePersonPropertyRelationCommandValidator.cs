using FluentValidation;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.CreatePersonPropertyRelation;

/// <summary>
/// Validator for CreatePersonPropertyRelationCommand
/// Standalone person-property relation creation (outside survey context)
/// Mirrors LinkPersonToPropertyUnitCommandValidator business rules
/// </summary>
public class CreatePersonPropertyRelationCommandValidator : AbstractValidator<CreatePersonPropertyRelationCommand>
{
    public CreatePersonPropertyRelationCommandValidator()
    {
        // ==================== REQUIRED IDs ====================

        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required");

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty().WithMessage("Property unit ID is required");

        // ==================== RELATION TYPE ====================

        RuleFor(x => x.RelationType)
            .IsInEnum()
            .WithMessage("Invalid relation type. Valid values: Owner, Occupant, Tenant, Guest, Heir, Other");

        RuleFor(x => x.RelationTypeOtherDesc)
            .NotEmpty()
            .When(x => x.RelationType == RelationType.Other)
            .WithMessage("Description is required when relation type is 'Other'");

        RuleFor(x => x.RelationTypeOtherDesc)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.RelationTypeOtherDesc));

        // ==================== CONTRACT TYPE ====================

        RuleFor(x => x.ContractType)
            .IsInEnum()
            .When(x => x.ContractType.HasValue)
            .WithMessage("Invalid contract type");

        RuleFor(x => x.ContractTypeOtherDesc)
            .NotEmpty()
            .When(x => x.ContractType == TenureContractType.Other)
            .WithMessage("Description is required when contract type is 'Other'");

        RuleFor(x => x.ContractTypeOtherDesc)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ContractTypeOtherDesc));

        // ==================== OWNERSHIP SHARE ====================

        RuleFor(x => x.OwnershipShare)
            .NotNull()
            .When(x => x.RelationType == RelationType.Owner)
            .WithMessage("Ownership share is required for Owner relation type");

        RuleFor(x => x.OwnershipShare)
            .GreaterThan(0)
            .When(x => x.RelationType == RelationType.Owner && x.OwnershipShare.HasValue)
            .WithMessage("Ownership share must be greater than 0 for Owner relation type");

        RuleFor(x => x.OwnershipShare)
            .LessThanOrEqualTo(1)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage("Ownership share cannot exceed 1.0 (100%)");

        // ==================== TEXT FIELDS ====================

        RuleFor(x => x.ContractDetails)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.ContractDetails));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // ==================== DATE VALIDATIONS ====================

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date cannot be before start date");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.StartDate.HasValue &&
                       (x.RelationType == RelationType.Owner || x.RelationType == RelationType.Tenant))
            .WithMessage("Start date cannot be in the future for Owner or Tenant relations");
    }
}
