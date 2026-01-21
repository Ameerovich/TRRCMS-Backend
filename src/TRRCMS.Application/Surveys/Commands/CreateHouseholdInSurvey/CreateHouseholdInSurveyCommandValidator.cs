using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;

/// <summary>
/// Validator for CreateHouseholdInSurveyCommand
/// </summary>
public class CreateHouseholdInSurveyCommandValidator : AbstractValidator<CreateHouseholdInSurveyCommand>
{
    public CreateHouseholdInSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage("Property unit ID is required");

        RuleFor(x => x.HeadOfHouseholdName)
            .NotEmpty()
            .WithMessage("Head of household name is required")
            .MaximumLength(200)
            .WithMessage("Head of household name cannot exceed 200 characters");

        RuleFor(x => x.HouseholdSize)
            .GreaterThan(0)
            .WithMessage("Household size must be at least 1")
            .LessThanOrEqualTo(50)
            .WithMessage("Household size cannot exceed 50");

        // Gender composition validation
        RuleFor(x => x.MaleCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaleCount.HasValue)
            .WithMessage("Male count cannot be negative");

        RuleFor(x => x.FemaleCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.FemaleCount.HasValue)
            .WithMessage("Female count cannot be negative");

        // Age composition validation
        RuleFor(x => x.InfantCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.InfantCount.HasValue)
            .WithMessage("Infant count cannot be negative");

        RuleFor(x => x.ChildCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ChildCount.HasValue)
            .WithMessage("Child count cannot be negative");

        RuleFor(x => x.MinorCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinorCount.HasValue)
            .WithMessage("Minor count cannot be negative");

        RuleFor(x => x.AdultCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.AdultCount.HasValue)
            .WithMessage("Adult count cannot be negative");

        RuleFor(x => x.ElderlyCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ElderlyCount.HasValue)
            .WithMessage("Elderly count cannot be negative");

        // Vulnerability indicators validation
        RuleFor(x => x.PersonsWithDisabilitiesCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PersonsWithDisabilitiesCount.HasValue)
            .WithMessage("Persons with disabilities count cannot be negative");

        RuleFor(x => x.WidowCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.WidowCount.HasValue)
            .WithMessage("Widow count cannot be negative");

        RuleFor(x => x.OrphanCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.OrphanCount.HasValue)
            .WithMessage("Orphan count cannot be negative");

        RuleFor(x => x.SingleParentCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SingleParentCount.HasValue)
            .WithMessage("Single parent count cannot be negative");

        // Economic indicators validation
        RuleFor(x => x.EmployedPersonsCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.EmployedPersonsCount.HasValue)
            .WithMessage("Employed persons count cannot be negative");

        RuleFor(x => x.UnemployedPersonsCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.UnemployedPersonsCount.HasValue)
            .WithMessage("Unemployed persons count cannot be negative");

        RuleFor(x => x.PrimaryIncomeSource)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.PrimaryIncomeSource))
            .WithMessage("Primary income source cannot exceed 200 characters");

        RuleFor(x => x.MonthlyIncomeEstimate)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MonthlyIncomeEstimate.HasValue)
            .WithMessage("Monthly income estimate cannot be negative");

        // Displacement validation
        RuleFor(x => x.OriginLocation)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.OriginLocation))
            .WithMessage("Origin location cannot exceed 500 characters");

        RuleFor(x => x.DisplacementReason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplacementReason))
            .WithMessage("Displacement reason cannot exceed 1000 characters");

        RuleFor(x => x.ArrivalDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.ArrivalDate.HasValue)
            .WithMessage("Arrival date cannot be in the future");

        // Additional information validation
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.SpecialNeeds)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.SpecialNeeds))
            .WithMessage("Special needs cannot exceed 1000 characters");
    }
}