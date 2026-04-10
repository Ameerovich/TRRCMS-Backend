using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Streets.Commands.RegisterStreet;

public class RegisterStreetCommandValidator : LocalizedValidator<RegisterStreetCommand>
{
    public RegisterStreetCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Identifier)
            .GreaterThan(0).WithMessage(L("Identifier_PositiveInteger"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(L("StreetName_Required"))
            .MaximumLength(500).WithMessage(L("StreetName_MaxLength500"));

        RuleFor(x => x.GeometryWkt)
            .NotEmpty().WithMessage(L("StreetGeometry_Required"));
    }
}
