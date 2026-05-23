using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.PropertyUnits.Commands.UpdatePropertyUnit;

/// <summary>
/// Validator for UpdatePropertyUnitCommand
/// </summary>
public class UpdatePropertyUnitCommandValidator : LocalizedValidator<UpdatePropertyUnitCommand>
{
    public UpdatePropertyUnitCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(L("PropertyUnitId_Required"));

        RuleFor(x => x.UnitIdentifier!)
            .NotEmpty()
            .WithMessage(L("UnitId_Required"))
            .MaximumLength(50)
            .WithMessage(L("UnitId_MaxLength50"))
            .When(x => x.UnitIdentifier is not null);

        RuleFor(x => x.UnitType)
            .Must(v => vocabService.IsValidCode("property_unit_type", v!.Value))
            .When(x => x.UnitType.HasValue)
            .WithMessage(L("UnitType_InvalidRange"));

        RuleFor(x => x.Status)
            .Must(v => vocabService.IsValidCode("property_unit_status", v!.Value))
            .When(x => x.Status.HasValue)
            .WithMessage(L("UnitStatus_ValidValue"));

        RuleFor(x => x.FloorNumber)
            .InclusiveBetween(-5, 200)
            .When(x => x.FloorNumber.HasValue)
            .WithMessage(L("FloorNumber_Range"));

        RuleFor(x => x.AreaSquareMeters)
            .GreaterThan(0)
            .WithMessage(L("Area_GreaterThanZero"))
            .LessThanOrEqualTo(10000)
            .WithMessage(L("Area_MaxValue"))
            .When(x => x.AreaSquareMeters.HasValue);

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
