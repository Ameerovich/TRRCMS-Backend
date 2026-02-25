using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Application.Conflicts.Queries.GetConflictQueue;

namespace TRRCMS.Application.Conflicts.Queries.GetPropertyDuplicates;

/// <summary>
/// Handler for <see cref="GetPropertyDuplicatesQuery"/>.
/// Delegates to <see cref="IConflictResolutionRepository.SearchAsync"/> with
/// EntityType locked to "PropertyUnit" — catches both "PropertyDuplicate"
/// and "PropertyDuplicate_WithinBatch" conflict types.
///
/// UC-007 S01–S02: Display Potential Duplicate PropertyUnit Groups.
/// Returns property unit records flagged as potential duplicates during import,
/// keyed by composite key (BuildingCode 17-digit + UnitIdentifier).
/// </summary>
public class GetPropertyDuplicatesQueryHandler
    : IRequestHandler<GetPropertyDuplicatesQuery, GetConflictQueueResponse>
{
    private readonly IConflictResolutionRepository _conflictRepository;

    public GetPropertyDuplicatesQueryHandler(IConflictResolutionRepository conflictRepository)
    {
        _conflictRepository = conflictRepository
            ?? throw new ArgumentNullException(nameof(conflictRepository));
    }

    public async Task<GetConflictQueueResponse> Handle(
        GetPropertyDuplicatesQuery request,
        CancellationToken cancellationToken)
    {
        var (conflicts, totalCount) = await _conflictRepository.SearchAsync(
            importPackageId: request.ImportPackageId,
            entityType: "PropertyUnit",
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
