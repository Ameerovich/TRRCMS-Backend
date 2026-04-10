using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.PropertyUnits.Commands.CreatePropertyUnit;

/// <summary>
/// Validator for CreatePropertyUnitCommand
/// </summary>
public class CreatePropertyUnitCommandValidator : LocalizedValidator<CreatePropertyUnitCommand>
{
    public CreatePropertyUnitCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage(L("BuildingId_Required"));

        RuleFor(x => x.UnitIdentifier)
            .NotEmpty()
            .WithMessage(L("UnitId_Required"))
            .MaximumLength(50)
            .WithMessage(L("UnitId_MaxLength50"));

        RuleFor(x => x.UnitType)
            .InclusiveBetween(1, 5)
            .WithMessage(L("UnitType_Range1to5"));

        RuleFor(x => x.Status)
            .Must(s => s >= 1 && s <= 6 || s == 99)
            .WithMessage(L("UnitStatus_ValidValue"));

        RuleFor(x => x.FloorNumber)
            .InclusiveBetween(-5, 200)
            .When(x => x.FloorNumber.HasValue)
            .WithMessage(L("FloorNumber_Range"));

        RuleFor(x => x.AreaSquareMeters)
            .GreaterThan(0)
            .When(x => x.AreaSquareMeters.HasValue)
            .WithMessage(L("Area_GreaterThanZero"));

        RuleFor(x => x.NumberOfRooms)
            .InclusiveBetween(0, 100)
            .When(x => x.NumberOfRooms.HasValue)
            .WithMessage(L("Rooms_Range0to100"));

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage(L("Notes_MaxLength2000"));
    }
}
