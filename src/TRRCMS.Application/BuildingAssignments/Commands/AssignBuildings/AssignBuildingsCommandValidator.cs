using FluentValidation;

namespace TRRCMS.Application.BuildingAssignments.Commands.AssignBuildings;

/// <summary>
/// Validator for AssignBuildingsCommand
/// </summary>
public class AssignBuildingsCommandValidator : AbstractValidator<AssignBuildingsCommand>
{
    public AssignBuildingsCommandValidator()
    {
        RuleFor(x => x.FieldCollectorId)
            .NotEmpty()
            .WithMessage("Field collector ID is required");

        RuleFor(x => x.Buildings)
            .NotEmpty()
            .WithMessage("At least one building must be specified for assignment");

        RuleFor(x => x.Buildings)
            .Must(buildings => buildings.Count <= 100)
            .WithMessage("Cannot assign more than 100 buildings at once");

        RuleForEach(x => x.Buildings).ChildRules(building =>
        {
            building.RuleFor(b => b.BuildingId)
                .NotEmpty()
                .WithMessage("Building ID is required");

            building.RuleFor(b => b.RevisitReason)
                .NotEmpty()
                .When(b => b.PropertyUnitIdsForRevisit?.Any() == true)
                .WithMessage("Revisit reason is required when property units are specified for revisit");

            building.RuleFor(b => b.PropertyUnitIdsForRevisit)
                .Must(units => units == null || units.Count <= 50)
                .WithMessage("Cannot specify more than 50 property units for revisit");
        });

        RuleFor(x => x.Priority)
            .Must(p => p == "Normal" || p == "High" || p == "Urgent")
            .WithMessage("Priority must be Normal, High, or Urgent");

        RuleFor(x => x.TargetCompletionDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .When(x => x.TargetCompletionDate.HasValue)
            .WithMessage("Target completion date must be in the future");

        RuleFor(x => x.AssignmentNotes)
            .MaximumLength(2000)
            .WithMessage("Assignment notes cannot exceed 2000 characters");
    }
}
