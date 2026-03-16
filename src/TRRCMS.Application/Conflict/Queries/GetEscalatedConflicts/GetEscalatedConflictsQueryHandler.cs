using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Application.Conflicts.Queries.GetConflictQueue;

namespace TRRCMS.Application.Conflicts.Queries.GetEscalatedConflicts;

/// <summary>
/// Handler for <see cref="GetEscalatedConflictsQuery"/>.
///
/// Returns only escalated conflicts that are still pending resolution,
/// forming the senior review queue.
///
/// Default sort: EscalatedDate descending (most recently escalated first).
/// </summary>
public class GetEscalatedConflictsQueryHandler
    : IRequestHandler<GetEscalatedConflictsQuery, GetConflictQueueResponse>
{
    private readonly IConflictResolutionRepository _conflictRepository;

    public GetEscalatedConflictsQueryHandler(IConflictResolutionRepository conflictRepository)
    {
        _conflictRepository = conflictRepository
            ?? throw new ArgumentNullException(nameof(conflictRepository));
    }

    public async Task<GetConflictQueueResponse> Handle(
        GetEscalatedConflictsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _conflictRepository.GetQueryable()
            .Where(c => c.IsEscalated && c.Status == "PendingReview");

        if (!string.IsNullOrWhiteSpace(request.ConflictType))
            query = query.Where(c => c.ConflictType.StartsWith(request.ConflictType));

        if (!string.IsNullOrWhiteSpace(request.Priority))
            query = query.Where(c => c.Priority == request.Priority);

        if (request.ImportPackageId.HasValue)
            query = query.Where(c => c.ImportPackageId == request.ImportPackageId.Value);

        if (request.IsOverdue.HasValue)
            query = query.Where(c => c.IsOverdue == request.IsOverdue.Value);

        var totalCount = query.Count();

        query = request.SortBy?.ToLower() switch
        {
            "priority" => request.SortDescending
                ? query.OrderByDescending(c => c.Priority)
                : query.OrderBy(c => c.Priority),
            "similarityscore" => request.SortDescending
                ? query.OrderByDescending(c => c.SimilarityScore)
                : query.OrderBy(c => c.SimilarityScore),
            "detecteddate" => request.SortDescending
                ? query.OrderByDescending(c => c.DetectedDate)
                : query.OrderBy(c => c.DetectedDate),
            "conflictnumber" => request.SortDescending
                ? query.OrderByDescending(c => c.ConflictNumber)
                : query.OrderBy(c => c.ConflictNumber),
            _ => request.SortDescending
                ? query.OrderByDescending(c => c.EscalatedDate)
                : query.OrderBy(c => c.EscalatedDate)
        };

        var conflicts = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = conflicts.Select(c => new ConflictDto
        {
            Id = c.Id,
            ConflictNumber = c.ConflictNumber,
            ConflictType = c.ConflictType,
            EntityType = c.EntityType,
            FirstEntityId = c.FirstEntityId,
            SecondEntityId = c.SecondEntityId,
            FirstEntityIdentifier = c.FirstEntityIdentifier,
            SecondEntityIdentifier = c.SecondEntityIdentifier,
            SimilarityScore = c.SimilarityScore,
            ConfidenceLevel = c.ConfidenceLevel,
            Status = c.Status,
            ResolutionAction = c.ResolutionAction,
            Priority = c.Priority,
            IsEscalated = c.IsEscalated,
            IsOverdue = c.IsOverdue || c.CheckIfOverdue(),
            IsAutoDetected = c.IsAutoDetected,
            IsAutoResolved = c.IsAutoResolved,
            DetectedDate = c.DetectedDate,
            AssignedDate = c.AssignedDate,
            ResolvedDate = c.ResolvedDate,
            ImportPackageId = c.ImportPackageId,
            AssignedToUserId = c.AssignedToUserId,
            ResolvedByUserId = c.ResolvedByUserId
        }).ToList();

        return new GetConflictQueueResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
