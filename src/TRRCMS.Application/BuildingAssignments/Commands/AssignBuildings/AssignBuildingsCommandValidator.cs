using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.BuildingAssignments.Commands.AssignBuildings;

/// <summary>
/// Validator for AssignBuildingsCommand
/// </summary>
public class AssignBuildingsCommandValidator : LocalizedValidator<AssignBuildingsCommand>
{
    public AssignBuildingsCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.FieldCollectorId)
            .NotEmpty()
            .WithMessage(L("FieldCollectorId_Required"));

        RuleFor(x => x.Buildings)
            .NotEmpty()
            .WithMessage(L("Assignment_AtLeastOneBuilding"));

        RuleFor(x => x.Buildings)
            .Must(buildings => buildings.Count <= 100)
            .WithMessage(L("Assignment_Max100Buildings"));

        RuleForEach(x => x.Buildings).ChildRules(building =>
        {
            building.RuleFor(b => b.BuildingId)
                .NotEmpty()
                .WithMessage(L("BuildingId_Required"));

            building.RuleFor(b => b.RevisitReason)
                .NotEmpty()
                .When(b => b.PropertyUnitIdsForRevisit?.Any() == true)
                .WithMessage(L("RevisitReason_Required"));

            building.RuleFor(b => b.PropertyUnitIdsForRevisit)
                .Must(units => units == null || units.Count <= 50)
                .WithMessage(L("Revisit_Max50Units"));
        });

        RuleFor(x => x.Priority)
            .Must(p => p == "Normal" || p == "High" || p == "Urgent")
            .WithMessage(L("Priority_Invalid"));

        RuleFor(x => x.TargetCompletionDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .When(x => x.TargetCompletionDate.HasValue)
            .WithMessage(L("TargetDate_MustBeFuture"));

        RuleFor(x => x.AssignmentNotes)
            .MaximumLength(2000)
            .WithMessage(L("AssignmentNotes_MaxLength2000"));
    }
}
