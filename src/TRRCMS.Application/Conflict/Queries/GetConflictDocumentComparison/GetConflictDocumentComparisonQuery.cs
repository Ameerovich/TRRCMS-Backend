using MediatR;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictDocumentComparison;

/// <summary>
/// Query to retrieve documents and evidence for both entities in a conflict pair,
/// enabling side-by-side document comparison in the review UI.
///
/// UC-008 S04: Document viewer data for person duplicate review.
/// Also supports UC-007 for property unit document comparison.
/// </summary>
public sealed record GetConflictDocumentComparisonQuery(Guid ConflictId)
    : IRequest<DocumentComparisonDto?>;
