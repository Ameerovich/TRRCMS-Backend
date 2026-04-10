using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Surveys.Commands.UpdatePropertyUnitInSurvey;

/// <summary>
/// Validator for UpdatePropertyUnitInSurveyCommand
/// </summary>
public class UpdatePropertyUnitInSurveyCommandValidator : LocalizedValidator<UpdatePropertyUnitInSurveyCommand>
{
    public UpdatePropertyUnitInSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage(L("PropertyUnitId_Required"));

        RuleFor(x => x.UnitIdentifier)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.UnitIdentifier))
            .WithMessage(L("UnitIdentifier_MaxLength50"));

        RuleFor(x => x.UnitType)
            .InclusiveBetween(1, 5)
            .When(x => x.UnitType.HasValue)
            .WithMessage(L("UnitType_InvalidRange"));

        RuleFor(x => x.Status)
            .Must(s => !s.HasValue || (s.Value >= 1 && s.Value <= 6) || s.Value == 99)
            .WithMessage(L("UnitStatus_Invalid"));

        RuleFor(x => x.FloorNumber)
            .InclusiveBetween(-5, 200)
            .When(x => x.FloorNumber.HasValue)
            .WithMessage(L("FloorNumber_InvalidRange"));

        RuleFor(x => x.AreaSquareMeters)
            .GreaterThan(0)
            .When(x => x.AreaSquareMeters.HasValue)
            .WithMessage(L("Area_GreaterThanZero"));

        RuleFor(x => x.NumberOfRooms)
            .InclusiveBetween(0, 100)
            .When(x => x.NumberOfRooms.HasValue)
            .WithMessage(L("NumberOfRooms_InvalidRange"));

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage(L("Description_MaxLength2000"));
    }
}
