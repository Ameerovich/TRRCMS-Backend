using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.CreatePropertyUnitInSurvey;

/// <summary>
/// Validator for CreatePropertyUnitInSurveyCommand
/// </summary>
public class CreatePropertyUnitInSurveyCommandValidator : AbstractValidator<CreatePropertyUnitInSurveyCommand>
{
    public CreatePropertyUnitInSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.UnitIdentifier)
            .NotEmpty()
            .WithMessage("Unit identifier is required")
            .MaximumLength(50)
            .WithMessage("Unit identifier cannot exceed 50 characters");

        RuleFor(x => x.UnitType)
            .NotEmpty()
            .WithMessage("Unit type is required")
            .MaximumLength(100)
            .WithMessage("Unit type cannot exceed 100 characters");

        RuleFor(x => x.FloorNumber)
            .GreaterThanOrEqualTo(-5)
            .When(x => x.FloorNumber.HasValue)
            .WithMessage("Floor number must be at least -5 (basement levels)")
            .LessThanOrEqualTo(50)
            .When(x => x.FloorNumber.HasValue)
            .WithMessage("Floor number cannot exceed 50");

        RuleFor(x => x.PositionOnFloor)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.PositionOnFloor))
            .WithMessage("Position on floor cannot exceed 100 characters");

        RuleFor(x => x.OccupancyStatus)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.OccupancyStatus))
            .WithMessage("Occupancy status cannot exceed 50 characters");

        RuleFor(x => x.NumberOfRooms)
            .GreaterThan(0)
            .When(x => x.NumberOfRooms.HasValue)
            .WithMessage("Number of rooms must be greater than 0")
            .LessThanOrEqualTo(100)
            .When(x => x.NumberOfRooms.HasValue)
            .WithMessage("Number of rooms cannot exceed 100");

        RuleFor(x => x.EstimatedAreaSqm)
            .GreaterThan(0)
            .When(x => x.EstimatedAreaSqm.HasValue)
            .WithMessage("Estimated area must be greater than 0")
            .LessThanOrEqualTo(10000)
            .When(x => x.EstimatedAreaSqm.HasValue)
            .WithMessage("Estimated area cannot exceed 10,000 square meters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.UtilitiesNotes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.UtilitiesNotes))
            .WithMessage("Utilities notes cannot exceed 1000 characters");
    }
}