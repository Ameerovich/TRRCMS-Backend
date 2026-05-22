using MediatR;

namespace TRRCMS.Application.Import.Queries.GetReconciliationQueue;

/// <summary>
/// Query for records that the import pipeline committed with an automatic adjustment to satisfy a
/// uniqueness rule after an operator chose Keep-Separate, and which therefore await manual review:
///   - Persons whose National ID was cleared (a NID duplicate was kept separate).
///   - Property units whose UnitIdentifier was suffix-disambiguated (a composite-key duplicate was kept separate).
///
/// Read-only. Lets a dashboard surface a "needs reconciliation" queue. Existing endpoints are unaffected.
/// </summary>
public record GetReconciliationQueueQuery : IRequest<ReconciliationQueueDto>
{
    /// <summary>Which adjustments to include: "person", "propertyunit", or null/empty for both.</summary>
    public string? EntityTypeFilter { get; init; }

    /// <summary>1-based page number (defaults to 1).</summary>
    public int Page { get; init; } = 1;

    /// <summary>Page size per entity type (defaults to 20).</summary>
    public int PageSize { get; init; } = 20;
}
