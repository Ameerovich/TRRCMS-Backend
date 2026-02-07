using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictQueue;

/// <summary>
/// Handler for <see cref="GetConflictQueueQuery"/>.
/// Delegates to <see cref="IConflictResolutionRepository.SearchAsync"/> for
/// paginated, filtered conflict queue retrieval.
/// </summary>
public class GetConflictQueueQueryHandler
    : IRequestHandler<GetConflictQueueQuery, GetConflictQueueResponse>
{
    private readonly IConflictResolutionRepository _conflictRepository;

    public GetConflictQueueQueryHandler(IConflictResolutionRepository conflictRepository)
    {
        _conflictRepository = conflictRepository
            ?? throw new ArgumentNullException(nameof(conflictRepository));
    }

    public async Task<GetConflictQueueResponse> Handle(
        GetConflictQueueQuery request,
        CancellationToken cancellationToken)
    {
        var (conflicts, totalCount) = await _conflictRepository.SearchAsync(
            importPackageId: request.ImportPackageId,
            conflictType: request.ConflictType,
            status: request.Status,
            priority: request.Priority,
            assignedToUserId: request.AssignedToUserId,
            isEscalated: request.IsEscalated,
            isOverdue: request.IsOverdue,
            page: request.Page,
            pageSize: request.PageSize,
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            cancellationToken: cancellationToken);

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
