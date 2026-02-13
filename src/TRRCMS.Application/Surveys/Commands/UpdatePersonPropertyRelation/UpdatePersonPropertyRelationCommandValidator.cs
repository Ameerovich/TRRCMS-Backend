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

        // RelationType enum validation (optional for partial update)
        RuleFor(x => x.RelationType)
            .IsInEnum()
            .When(x => x.RelationType.HasValue)
            .WithMessage("Invalid relation type");

        // OccupancyType enum validation (optional)
        RuleFor(x => x.OccupancyType)
            .IsInEnum()
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
