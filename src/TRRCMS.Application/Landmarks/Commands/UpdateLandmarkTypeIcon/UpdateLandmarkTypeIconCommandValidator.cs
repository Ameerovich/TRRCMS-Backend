using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Landmarks.Commands.UpdateLandmarkTypeIcon;

public class UpdateLandmarkTypeIconCommandValidator : LocalizedValidator<UpdateLandmarkTypeIconCommand>
{
    public UpdateLandmarkTypeIconCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Type)
            .InclusiveBetween(1, 10)
            .WithMessage(L("LandmarkType_Range1to10"));

        RuleFor(x => x.SvgContent)
            .NotEmpty()
            .WithMessage(L("SvgContent_Required"))
            .Must(svg => svg.Contains("<svg", StringComparison.OrdinalIgnoreCase))
            .WithMessage(L("SvgContent_InvalidTag"))
            .MaximumLength(102400)
            .WithMessage(L("SvgContent_Max100KB"));
    }
}
