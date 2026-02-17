using FluentValidation;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.CreatePersonPropertyRelation;

/// <summary>
/// Validator for CreatePersonPropertyRelationCommand
/// Standalone person-property relation creation (outside survey context)
/// Mirrors LinkPersonToPropertyUnitCommandValidator business rules
/// </summary>
public class CreatePersonPropertyRelationCommandValidator : AbstractValidator<CreatePersonPropertyRelationCommand>
{
    public CreatePersonPropertyRelationCommandValidator(IVocabularyValidationService vocabService)
    {
        // ==================== REQUIRED IDs ====================

        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("Person ID is required");

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty().WithMessage("Property unit ID is required");

        // ==================== RELATION TYPE ====================

        RuleFor(x => x.RelationType)
            .Must(v => vocabService.IsValidCode("relation_type", (int)v))
            .WithMessage("Invalid relation type. Valid values: Owner, Occupant, Tenant, Guest, Heir, Other");

        // ==================== OCCUPANCY TYPE ====================

        RuleFor(x => x.OccupancyType)
            .Must(v => vocabService.IsValidCode("occupancy_type", (int)v!.Value))
            .When(x => x.OccupancyType.HasValue)
            .WithMessage("Invalid occupancy type");

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
    }
}
