using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Landmarks.Commands.RegisterLandmark;

public class RegisterLandmarkCommandValidator : LocalizedValidator<RegisterLandmarkCommand>
{
    public RegisterLandmarkCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Identifier)
            .GreaterThan(0).WithMessage(L("Identifier_PositiveInteger"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(L("LandmarkName_Required"))
            .MaximumLength(500).WithMessage(L("LandmarkName_MaxLength500"));

        RuleFor(x => x.Type)
            .InclusiveBetween(1, 10).WithMessage(L("LandmarkType_Range1to10"));

        RuleFor(x => x.LocationWkt)
            .NotEmpty().WithMessage(L("LandmarkGeometry_Required"));
    }
}
