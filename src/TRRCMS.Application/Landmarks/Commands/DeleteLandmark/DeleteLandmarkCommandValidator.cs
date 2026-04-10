using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Landmarks.Commands.DeleteLandmark;

public class DeleteLandmarkCommandValidator : LocalizedValidator<DeleteLandmarkCommand>
{
    public DeleteLandmarkCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(L("LandmarkId_Required"));
    }
}
