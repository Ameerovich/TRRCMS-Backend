using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.PropertyUnits.Commands.UpdatePropertyUnit;

/// <summary>
/// Validator for UpdatePropertyUnitCommand
/// </summary>
public class UpdatePropertyUnitCommandValidator : LocalizedValidator<UpdatePropertyUnitCommand>
{
    public UpdatePropertyUnitCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(L("PropertyUnitId_Required"));

        RuleFor(x => x.UnitType)
            .InclusiveBetween(1, 5)
            .When(x => x.UnitType.HasValue)
            .WithMessage(L("UnitType_Range1to5"));

        RuleFor(x => x.Status)
            .Must(s => !s.HasValue || (s.Value >= 1 && s.Value <= 6) || s.Value == 99)
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
