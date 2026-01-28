using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UpdatePropertyUnitInSurvey;

/// <summary>
/// Validator for UpdatePropertyUnitInSurveyCommand
/// </summary>
public class UpdatePropertyUnitInSurveyCommandValidator : AbstractValidator<UpdatePropertyUnitInSurveyCommand>
{
    public UpdatePropertyUnitInSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage("Property unit ID is required");

        RuleFor(x => x.UnitType)
            .InclusiveBetween(1, 5)
            .When(x => x.UnitType.HasValue)
            .WithMessage("Unit type (نوع الوحدة) must be between 1 and 5");

        RuleFor(x => x.Status)
            .Must(s => !s.HasValue || (s.Value >= 1 && s.Value <= 6) || s.Value == 99)
            .WithMessage("Status (حالة الوحدة) must be a valid value (1-6 or 99)");

        RuleFor(x => x.FloorNumber)
            .InclusiveBetween(-5, 200)
            .When(x => x.FloorNumber.HasValue)
            .WithMessage("Floor number (رقم الطابق) must be between -5 and 200");

        RuleFor(x => x.AreaSquareMeters)
            .GreaterThan(0)
            .When(x => x.AreaSquareMeters.HasValue)
            .WithMessage("Area (مساحة القسم) must be greater than 0");

        RuleFor(x => x.NumberOfRooms)
            .InclusiveBetween(0, 100)
            .When(x => x.NumberOfRooms.HasValue)
            .WithMessage("Number of rooms (عدد الغرف) must be between 0 and 100");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description (وصف مفصل) must not exceed 2000 characters");
    }
}
