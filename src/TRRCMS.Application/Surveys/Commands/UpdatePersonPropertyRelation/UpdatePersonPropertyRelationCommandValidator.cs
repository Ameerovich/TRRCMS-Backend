using FluentValidation;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdatePersonPropertyRelation;

public class UpdatePersonPropertyRelationCommandValidator : AbstractValidator<UpdatePersonPropertyRelationCommand>
{
    public UpdatePersonPropertyRelationCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.RelationId)
            .NotEmpty()
            .WithMessage("Relation ID is required");

        // RelationType enum validation (optional int field for partial update)
        RuleFor(x => x.RelationType)
            .Must(v => Enum.IsDefined(typeof(RelationType), v!.Value))
            .When(x => x.RelationType.HasValue)
            .WithMessage("Invalid relation type");

        // OccupancyType enum validation (optional int field)
        RuleFor(x => x.OccupancyType)
            .Must(v => Enum.IsDefined(typeof(OccupancyType), v!.Value))
            .When(x => x.OccupancyType.HasValue)
            .WithMessage("Invalid occupancy type");

        // Ownership share validation
        RuleFor(x => x.OwnershipShare)
            .GreaterThan(0)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage("Ownership share must be greater than 0");

        RuleFor(x => x.OwnershipShare)
            .LessThanOrEqualTo(1)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage("Ownership share cannot exceed 1.0 (100%)");

        // Text field max lengths
        RuleFor(x => x.ContractDetails)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.ContractDetails));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
