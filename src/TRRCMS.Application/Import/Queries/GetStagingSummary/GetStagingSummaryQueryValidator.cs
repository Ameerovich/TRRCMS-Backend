using FluentValidation;

namespace TRRCMS.Application.Import.Queries.GetStagingSummary;

public class GetStagingSummaryQueryValidator : AbstractValidator<GetStagingSummaryQuery>
{
    public GetStagingSummaryQueryValidator()
    {
        RuleFor(x => x.ImportPackageId)
            .NotEmpty()
            .WithMessage("Import package ID is required");
    }
}
