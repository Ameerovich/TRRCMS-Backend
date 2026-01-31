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

        // ContractType enum validation (optional)
        RuleFor(x => x.ContractType)
            .IsInEnum()
            .When(x => x.ContractType.HasValue)
            .WithMessage("Invalid contract type");

        // Ownership share validation
        RuleFor(x => x.OwnershipShare)
            .GreaterThan(0)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage("Ownership share must be greater than 0");

        RuleFor(x => x.OwnershipShare)
            .LessThanOrEqualTo(1)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage("Ownership share cannot exceed 1.0 (100%)");

        // RelationTypeOtherDesc max length
        RuleFor(x => x.RelationTypeOtherDesc)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.RelationTypeOtherDesc));

        // ContractTypeOtherDesc max length
        RuleFor(x => x.ContractTypeOtherDesc)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ContractTypeOtherDesc));

        RuleFor(x => x.ContractDetails)
            .MaximumLength(2000);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);

        // Date validation (when both provided in update)
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date cannot be before start date");
    }
}
