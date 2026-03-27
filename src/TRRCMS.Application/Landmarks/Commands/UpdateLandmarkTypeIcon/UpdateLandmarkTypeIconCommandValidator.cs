using FluentValidation;

namespace TRRCMS.Application.Landmarks.Commands.UpdateLandmarkTypeIcon;

public class UpdateLandmarkTypeIconCommandValidator : AbstractValidator<UpdateLandmarkTypeIconCommand>
{
    public UpdateLandmarkTypeIconCommandValidator()
    {
        RuleFor(x => x.Type)
            .InclusiveBetween(1, 10)
            .WithMessage("Landmark type must be between 1 and 10.");

        RuleFor(x => x.SvgContent)
            .NotEmpty()
            .WithMessage("SVG content is required.")
            .Must(svg => svg.Contains("<svg", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Content must be valid SVG (must contain <svg tag).")
            .MaximumLength(102400)
            .WithMessage("SVG content must not exceed 100KB.");
    }
}
