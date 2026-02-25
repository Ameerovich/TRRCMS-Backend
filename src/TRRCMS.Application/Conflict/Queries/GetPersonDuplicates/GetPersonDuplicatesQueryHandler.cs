using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Application.Conflicts.Queries.GetConflictQueue;

namespace TRRCMS.Application.Conflicts.Queries.GetPersonDuplicates;

/// <summary>
/// Handler for <see cref="GetPersonDuplicatesQuery"/>.
/// Delegates to <see cref="IConflictResolutionRepository.SearchAsync"/> with
/// ConflictType locked to "PersonDuplicate".
///
/// Display Potential Duplicate Persons.
/// Returns person records sharing the same national_id value,
/// flagged during import or cross-batch detection.
///
/// Also includes within-batch duplicates (PersonDuplicate_WithinBatch)
/// by matching on the "PersonDuplicate" prefix.
/// </summary>
public class GetPersonDuplicatesQueryHandler
    : IRequestHandler<GetPersonDuplicatesQuery, GetConflictQueueResponse>
{
    private readonly IConflictResolutionRepository _conflictRepository;

    public GetPersonDuplicatesQueryHandler(IConflictResolutionRepository conflictRepository)
    {
        _conflictRepository = conflictRepository
            ?? throw new ArgumentNullException(nameof(conflictRepository));
    }

    public async Task<GetConflictQueueResponse> Handle(
        GetPersonDuplicatesQuery request,
        CancellationToken cancellationToken)
    {
        // Include both "PersonDuplicate" and "PersonDuplicate_WithinBatch"
        // by querying on the base type. The repository SearchAsync uses exact match,
        // so we query the broader set via IQueryable for prefix matching.
        var query = _conflictRepository.GetQueryable()
            .Where(c => c.ConflictType.StartsWith("PersonDuplicate"));

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(c => c.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.Priority))
            query = query.Where(c => c.Priority == request.Priority);

        if (request.ImportPackageId.HasValue)
            query = query.Where(c => c.ImportPackageId == request.ImportPackageId.Value);

        if (request.AssignedToUserId.HasValue)
            query = query.Where(c => c.AssignedToUserId == request.AssignedToUserId.Value);

        if (request.IsEscalated.HasValue)
            query = query.Where(c => c.IsEscalated == request.IsEscalated.Value);

        if (request.IsOverdue.HasValue)
            query = query.Where(c => c.IsOverdue == request.IsOverdue.Value);

        // Total count before pagination
        var totalCount = query.Count();

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "similarityscore" => request.SortDescending
                ? query.OrderByDescending(c => c.SimilarityScore)
                : query.OrderBy(c => c.SimilarityScore),
            "priority" => request.SortDescending
                ? query.OrderByDescending(c => c.Priority)
                : query.OrderBy(c => c.Priority),
            "conflictnumber" => request.SortDescending
                ? query.OrderByDescending(c => c.ConflictNumber)
                : query.OrderBy(c => c.ConflictNumber),
            _ => request.SortDescending
                ? query.OrderByDescending(c => c.DetectedDate)
                : query.OrderBy(c => c.DetectedDate)
        };

        // Pagination
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
