using MediatR;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictDocumentComparison;

/// <summary>
/// Query to retrieve documents and evidence for both entities in a conflict pair,
/// enabling side-by-side document comparison in the review UI.
/// </summary>
public sealed record GetConflictDocumentComparisonQuery(Guid ConflictId)
    : IRequest<DocumentComparisonDto?>;
