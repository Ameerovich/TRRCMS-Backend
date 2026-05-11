using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictSummary;

/// <summary>
/// Handler for <see cref="GetConflictSummaryQuery"/>.
/// Aggregates conflict counts by type, status, and flags using
/// <see cref="IConflictResolutionRepository"/> aggregate queries.
/// </summary>
public class GetConflictSummaryQueryHandler
    : IRequestHandler<GetConflictSummaryQuery, ConflictSummaryDto>
{
    private readonly IConflictResolutionRepository _conflictRepository;

    public GetConflictSummaryQueryHandler(IConflictResolutionRepository conflictRepository)
    {
        _conflictRepository = conflictRepository
            ?? throw new ArgumentNullException(nameof(conflictRepository));
    }

    public async Task<ConflictSummaryDto> Handle(
        GetConflictSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var statusCounts = await _conflictRepository.GetStatusCountsAsync(cancellationToken);
        var typeCounts = await _conflictRepository.GetTypeCountsAsync(cancellationToken);

        var pendingPersonDuplicates = await GetPendingCountByTypeAsync(
            "PersonDuplicate", cancellationToken);
        var pendingPropertyDuplicates = await GetPendingCountByTypeAsync(
            "PropertyDuplicate", cancellationToken);
        var pendingClaimConflicts = await GetPendingCountByTypeAsync(
            "ClaimConflict", cancellationToken);

        var queryable = _conflictRepository.GetQueryable();

        var escalatedCount = queryable
            .Count(c => c.IsEscalated && c.Status == "PendingReview" && !c.IsDeleted);
        var highPriorityCount = queryable
            .Count(c => c.Priority == "High" && c.Status == "PendingReview" && !c.IsDeleted);
        var autoResolvedCount = queryable
            .Count(c => c.IsAutoResolved && !c.IsDeleted);
        var overdueCount = queryable
            .Count(c => c.IsOverdue && c.Status == "PendingReview" && !c.IsDeleted);

        // Sum both exact and _WithinBatch variants so totals are consistent with pending sub-counts.
        var personDuplicateCount =
            typeCounts.GetValueOrDefault("PersonDuplicate", 0) +
            typeCounts.GetValueOrDefault("PersonDuplicate_WithinBatch", 0);
        var propertyDuplicateCount =
            typeCounts.GetValueOrDefault("PropertyDuplicate", 0) +
            typeCounts.GetValueOrDefault("PropertyDuplicate_WithinBatch", 0);
        var claimConflictCount = typeCounts.GetValueOrDefault("ClaimConflict", 0);

        return new ConflictSummaryDto
        {
            TotalConflicts = await _conflictRepository.GetTotalCountAsync(cancellationToken),
            PendingReviewCount = statusCounts.GetValueOrDefault("PendingReview", 0),
            ResolvedCount = statusCounts.GetValueOrDefault("Resolved", 0),
            IgnoredCount = statusCounts.GetValueOrDefault("Ignored", 0),
            PersonDuplicateCount = personDuplicateCount,
            PropertyDuplicateCount = propertyDuplicateCount,
            ClaimConflictCount = claimConflictCount,
            PendingPersonDuplicates = pendingPersonDuplicates,
            PendingPropertyDuplicates = pendingPropertyDuplicates,
            PendingClaimConflicts = pendingClaimConflicts,
            EscalatedCount = escalatedCount,
            OverdueCount = overdueCount,
            HighPriorityCount = highPriorityCount,
            AutoResolvedCount = autoResolvedCount
        };
    }

    private async Task<int> GetPendingCountByTypeAsync(
        string conflictTypePrefix, CancellationToken cancellationToken)
    {
        var conflicts = await _conflictRepository.GetByConflictTypePrefixAndStatusAsync(
            conflictTypePrefix, "PendingReview", cancellationToken);
        return conflicts.Count;
    }
}
