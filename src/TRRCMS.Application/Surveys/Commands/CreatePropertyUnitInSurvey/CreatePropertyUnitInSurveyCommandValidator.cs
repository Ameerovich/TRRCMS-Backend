using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.CreatePropertyUnitInSurvey;

/// <summary>
/// Validator for CreatePropertyUnitInSurveyCommand
/// </summary>
public class CreatePropertyUnitInSurveyCommandValidator : LocalizedValidator<CreatePropertyUnitInSurveyCommand>
{
    public CreatePropertyUnitInSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.UnitIdentifier)
            .NotEmpty()
            .WithMessage(L("UnitIdentifier_Required"))
            .MaximumLength(50)
            .WithMessage(L("UnitIdentifier_MaxLength50"));

        RuleFor(x => x.UnitType)
            .InclusiveBetween(1, 5)
            .WithMessage(L("UnitType_InvalidRange"));

        RuleFor(x => x.Status)
            .Must(s => s >= 1 && s <= 6 || s == 99)
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
