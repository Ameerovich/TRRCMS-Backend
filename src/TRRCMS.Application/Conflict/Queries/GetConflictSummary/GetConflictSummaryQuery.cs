using MediatR;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictSummary;

/// <summary>
/// Query to retrieve aggregate conflict counts for the dashboard.
/// </summary>
public class GetConflictSummaryQuery : IRequest<ConflictSummaryDto>
{
}
