using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Queries.GetStagingSummary;

/// <summary>
/// Query to retrieve the current staging and validation summary for an import package.
/// This is a read-only query — it does not trigger validation, only reads current state.
/// </summary>
public record GetStagingSummaryQuery : IRequest<StagingSummaryDto>
{
    public Guid ImportPackageId { get; init; }
}
