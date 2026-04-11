using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.CreatePersonPropertyRelation;

/// <summary>
/// Validator for CreatePersonPropertyRelationCommand
/// Standalone person-property relation creation (outside survey context)
/// Mirrors LinkPersonToPropertyUnitCommandValidator business rules
/// </summary>
public class CreatePersonPropertyRelationCommandValidator : LocalizedValidator<CreatePersonPropertyRelationCommand>
{
    public CreatePersonPropertyRelationCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        // ==================== REQUIRED IDs ====================

        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage(L("PersonId_Required"));

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty().WithMessage(L("PropertyUnitId_Required"));

        // ==================== RELATION TYPE ====================

        RuleFor(x => x.RelationType)
            .Must(v => vocabService.IsValidCode("relation_type", (int)v))
            .WithMessage(L("RelationType_InvalidWithValues"));

        // ==================== OCCUPANCY TYPE ====================

        RuleFor(x => x.OccupancyType)
            .Must(v => vocabService.IsValidCode("occupancy_type", (int)v!.Value))
            .When(x => x.OccupancyType.HasValue)
            .WithMessage(L("OccupancyType_Invalid"));

        // ==================== OWNERSHIP SHARE ====================

        RuleFor(x => x.OwnershipShare)
            .NotNull()
            .When(x => x.RelationType == RelationType.Owner)
            .WithMessage(L("OwnershipShare_Required"));

        RuleFor(x => x.OwnershipShare)
            .GreaterThan(0)
            .When(x => x.RelationType == RelationType.Owner && x.OwnershipShare.HasValue)
            .WithMessage(L("OwnershipShare_GreaterThanZero"));

        RuleFor(x => x.OwnershipShare)
            .LessThanOrEqualTo(2400)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage(L("OwnershipShare_Max2400"));

        // ==================== TEXT FIELDS ====================

        RuleFor(x => x.ContractDetails)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.ContractDetails));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
