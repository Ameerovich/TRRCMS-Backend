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

        // Cross-query: pending count per conflict type
        var pendingPersonDuplicates = await GetPendingCountByTypeAsync(
            "PersonDuplicate", cancellationToken);
        var pendingPropertyDuplicates = await GetPendingCountByTypeAsync(
            "PropertyDuplicate", cancellationToken);
        var pendingClaimConflicts = await GetPendingCountByTypeAsync(
            "ClaimConflict", cancellationToken);

        // Flag-based counts via queryable
        var queryable = _conflictRepository.GetQueryable();

        var escalatedCount = queryable
            .Count(c => c.IsEscalated && c.Status == "PendingReview" && !c.IsDeleted);
        var highPriorityCount = queryable
            .Count(c => c.Priority == "High" && c.Status == "PendingReview" && !c.IsDeleted);
        var autoResolvedCount = queryable
            .Count(c => c.IsAutoResolved && !c.IsDeleted);
        var overdueCount = queryable
            .Count(c => c.IsOverdue && c.Status == "PendingReview" && !c.IsDeleted);

        return new ConflictSummaryDto
        {
            TotalConflicts = await _conflictRepository.GetTotalCountAsync(cancellationToken),
            PendingReviewCount = statusCounts.GetValueOrDefault("PendingReview", 0),
            ResolvedCount = statusCounts.GetValueOrDefault("Resolved", 0),
            IgnoredCount = statusCounts.GetValueOrDefault("Ignored", 0),
            PersonDuplicateCount = typeCounts.GetValueOrDefault("PersonDuplicate", 0),
            PropertyDuplicateCount = typeCounts.GetValueOrDefault("PropertyDuplicate", 0),
            ClaimConflictCount = typeCounts.GetValueOrDefault("ClaimConflict", 0),
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
        string conflictType, CancellationToken cancellationToken)
    {
        var conflicts = await _conflictRepository.GetByConflictTypeAndStatusAsync(
            conflictType, "PendingReview", cancellationToken);
        return conflicts.Count;
    }
}
