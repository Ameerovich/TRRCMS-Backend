using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;

/// <summary>
/// Validator for LinkPersonToPropertyUnitCommand
/// </summary>
public class LinkPersonToPropertyUnitCommandValidator : AbstractValidator<LinkPersonToPropertyUnitCommand>
{
    /// <summary>
    /// Valid relation types
    /// </summary>
    private static readonly string[] ValidRelationTypes = 
    { 
        "Owner", 
        "Tenant", 
        "Occupant", 
        "Heir", 
        "Guest", 
        "Other" 
    };

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

        RuleFor(x => x.RelationType)
            .NotEmpty()
            .WithMessage("Relation type is required")
            .Must(BeValidRelationType)
            .WithMessage($"Relation type must be one of: {string.Join(", ", ValidRelationTypes)}");

        // Ownership share validation for Owner type
        RuleFor(x => x.OwnershipShare)
            .GreaterThan(0)
            .When(x => x.RelationType.Equals("Owner", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Ownership share must be greater than 0 for Owner relation type")
            .LessThanOrEqualTo(1)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage("Ownership share cannot exceed 1.0 (100%)");

        // RelationTypeOtherDesc required when type is "Other"
        RuleFor(x => x.RelationTypeOtherDesc)
            .NotEmpty()
            .When(x => x.RelationType.Equals("Other", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Description is required when relation type is 'Other'")
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.RelationTypeOtherDesc))
            .WithMessage("Description cannot exceed 500 characters");

        // Contract details max length
        RuleFor(x => x.ContractDetails)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.ContractDetails))
            .WithMessage("Contract details cannot exceed 2000 characters");

        // Notes max length
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 2000 characters");

        // Date range validation
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date cannot be before start date");

        // Start date should not be in the future for Owner/Tenant
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)) // Allow 1 day buffer for timezone issues
            .When(x => x.StartDate.HasValue && 
                  (x.RelationType.Equals("Owner", StringComparison.OrdinalIgnoreCase) || 
                   x.RelationType.Equals("Tenant", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Start date cannot be in the future for Owner or Tenant relations");
    }

    /// <summary>
    /// Validates that the relation type is one of the allowed values
    /// </summary>
    private static bool BeValidRelationType(string relationType)
    {
        if (string.IsNullOrWhiteSpace(relationType))
            return false;

        return ValidRelationTypes.Any(t => t.Equals(relationType, StringComparison.OrdinalIgnoreCase));
    }
}
