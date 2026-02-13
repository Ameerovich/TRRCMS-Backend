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

        // OccupancyType enum validation (optional field)
        RuleFor(x => x.OccupancyType)
            .IsInEnum()
            .When(x => x.OccupancyType.HasValue)
            .WithMessage("Invalid occupancy type");

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

        // Text field max lengths
        RuleFor(x => x.ContractDetails)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.ContractDetails));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
