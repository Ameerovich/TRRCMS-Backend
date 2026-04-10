using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Import.Queries.GetStagingSummary;

public class GetStagingSummaryQueryValidator : LocalizedValidator<GetStagingSummaryQuery>
{
    public GetStagingSummaryQueryValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage(L("ImportPackageId_Required"));
    }
}
