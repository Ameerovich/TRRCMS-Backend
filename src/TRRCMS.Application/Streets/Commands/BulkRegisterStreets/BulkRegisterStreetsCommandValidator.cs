using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Streets.Commands.BulkRegisterStreets;

public class BulkRegisterStreetsCommandValidator : LocalizedValidator<BulkRegisterStreetsCommand>
{
    public BulkRegisterStreetsCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Streets)
            .NotEmpty().WithMessage(L("Street_AtLeastOne"));

        RuleForEach(x => x.Streets).ChildRules(item =>
        {
            item.RuleFor(x => x.Identifier)
                .GreaterThan(0).WithMessage(L("Identifier_PositiveInteger"));

            item.RuleFor(x => x.Name)
                .NotEmpty().WithMessage(L("StreetName_Required"))
                .MaximumLength(200).WithMessage(L("StreetName_MaxLength200"));

            item.RuleFor(x => x.GeometryWkt)
                .NotEmpty().WithMessage(L("StreetGeometry_BulkRequired"));
        });
    }
}
