using FluentValidation;

namespace TRRCMS.Application.PropertyUnits.Commands.CreatePropertyUnit;

/// <summary>
/// Validator for CreatePropertyUnitCommand
/// </summary>
public class CreatePropertyUnitCommandValidator : AbstractValidator<CreatePropertyUnitCommand>
{
    public CreatePropertyUnitCommandValidator()
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage("Building ID (معرف البناء) is required");

        RuleFor(x => x.UnitIdentifier)
            .NotEmpty()
            .WithMessage("Unit identifier (رقم الوحدة) is required")
            .MaximumLength(50)
            .WithMessage("Unit identifier must not exceed 50 characters");

        RuleFor(x => x.UnitType)
            .InclusiveBetween(1, 5)
            .WithMessage("Unit type (نوع الوحدة) must be between 1 and 5");

        RuleFor(x => x.Status)
            .Must(s => s >= 1 && s <= 6 || s == 99)
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
