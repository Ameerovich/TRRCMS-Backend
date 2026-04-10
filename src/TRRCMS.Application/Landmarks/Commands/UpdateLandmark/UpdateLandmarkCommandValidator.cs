using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Landmarks.Commands.UpdateLandmark;

public class UpdateLandmarkCommandValidator : LocalizedValidator<UpdateLandmarkCommand>
{
    public UpdateLandmarkCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(L("LandmarkId_Required"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(L("LandmarkName_Required"))
            .MaximumLength(500).WithMessage(L("LandmarkName_MaxLength500"));

        RuleFor(x => x.Type)
            .InclusiveBetween(1, 10).WithMessage(L("LandmarkType_Range1to10"));
    }
}
