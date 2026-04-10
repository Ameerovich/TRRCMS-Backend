using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Landmarks.Commands.BulkRegisterLandmarks;

public class BulkRegisterLandmarksCommandValidator : LocalizedValidator<BulkRegisterLandmarksCommand>
{
    public BulkRegisterLandmarksCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Landmarks)
            .NotEmpty().WithMessage(L("Landmark_AtLeastOne"));

        RuleForEach(x => x.Landmarks).ChildRules(item =>
        {
            item.RuleFor(x => x.Identifier)
                .GreaterThan(0).WithMessage(L("Identifier_PositiveInteger"));

            item.RuleFor(x => x.Name)
                .NotEmpty().WithMessage(L("LandmarkName_Required"))
                .MaximumLength(200).WithMessage(L("LandmarkName_MaxLength200"));

            item.RuleFor(x => x.Type)
                .InclusiveBetween(1, 10).WithMessage(L("LandmarkType_Range1to10"));

            item.RuleFor(x => x.LocationWkt)
                .NotEmpty().WithMessage(L("LandmarkGeometry_BulkRequired"));
        });
    }
}
