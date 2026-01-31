using FluentValidation;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;

public class LinkPersonToPropertyUnitCommandValidator : AbstractValidator<LinkPersonToPropertyUnitCommand>
{
    public LinkPersonToPropertyUnitCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.PersonId)
            .NotEmpty()
            .WithMessage("Person ID is required");

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage("Property Unit ID is required");

        // RelationType enum validation
        RuleFor(x => x.RelationType)
            .IsInEnum()
            .WithMessage("Invalid relation type. Valid values: Owner, Occupant, Tenant, Guest, Heir, Other");

        // ContractType enum validation (optional field)
        RuleFor(x => x.ContractType)
            .IsInEnum()
            .When(x => x.ContractType.HasValue)
            .WithMessage("Invalid contract type");

        // Ownership share required for Owner
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

        // RelationTypeOtherDesc required when RelationType is Other
        RuleFor(x => x.RelationTypeOtherDesc)
            .NotEmpty()
            .When(x => x.RelationType == RelationType.Other)
            .WithMessage("Description is required when relation type is 'Other'");

        RuleFor(x => x.RelationTypeOtherDesc)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.RelationTypeOtherDesc));

        // ContractTypeOtherDesc required when ContractType is Other
        RuleFor(x => x.ContractTypeOtherDesc)
            .NotEmpty()
            .When(x => x.ContractType == TenureContractType.Other)
            .WithMessage("Description is required when contract type is 'Other'");

        RuleFor(x => x.ContractTypeOtherDesc)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ContractTypeOtherDesc));

        RuleFor(x => x.ContractDetails)
            .MaximumLength(2000);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);

        // Date validation
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date cannot be before start date");

        // Start date not in future for Owner/Tenant
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.StartDate.HasValue &&
                       (x.RelationType == RelationType.Owner || x.RelationType == RelationType.Tenant))
            .WithMessage("Start date cannot be in the future for Owner or Tenant relations");
    }
}
