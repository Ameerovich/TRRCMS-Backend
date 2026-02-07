using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictDetails;

/// <summary>
/// Handler for <see cref="GetConflictDetailsQuery"/>.
/// Retrieves a single conflict with all fields for the review screen,
/// including DataComparison, MatchingCriteria, ReviewHistory, and computed ElapsedTime.
/// </summary>
public class GetConflictDetailsQueryHandler
    : IRequestHandler<GetConflictDetailsQuery, ConflictDetailDto>
{
    private readonly IConflictResolutionRepository _conflictRepository;

    public GetConflictDetailsQueryHandler(IConflictResolutionRepository conflictRepository)
    {
        _conflictRepository = conflictRepository
            ?? throw new ArgumentNullException(nameof(conflictRepository));
    }

    public async Task<ConflictDetailDto> Handle(
        GetConflictDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var conflict = await _conflictRepository.GetByIdAsync(request.Id, cancellationToken);

        if (conflict is null)
        {
            throw new InvalidOperationException(
                $"Conflict resolution with ID {request.Id} not found.");
        }

        return new ConflictDetailDto
        {
            Id = conflict.Id,
            ConflictNumber = conflict.ConflictNumber,
            ConflictType = conflict.ConflictType,
            EntityType = conflict.EntityType,
            ConflictDescription = conflict.ConflictDescription,
            FirstEntityId = conflict.FirstEntityId,
            SecondEntityId = conflict.SecondEntityId,
            FirstEntityIdentifier = conflict.FirstEntityIdentifier,
            SecondEntityIdentifier = conflict.SecondEntityIdentifier,
            SimilarityScore = conflict.SimilarityScore,
            ConfidenceLevel = conflict.ConfidenceLevel,
            MatchingCriteria = conflict.MatchingCriteria,
            DataComparison = conflict.DataComparison,
            Status = conflict.Status,
            ResolutionAction = conflict.ResolutionAction,
            Priority = conflict.Priority,
            IsEscalated = conflict.IsEscalated,
            IsOverdue = conflict.IsOverdue || conflict.CheckIfOverdue(),
            IsAutoDetected = conflict.IsAutoDetected,
            IsAutoResolved = conflict.IsAutoResolved,
            AutoResolutionRule = conflict.AutoResolutionRule,
            DetectedDate = conflict.DetectedDate,
            DetectedByUserId = conflict.DetectedByUserId,
            AssignedDate = conflict.AssignedDate,
            AssignedToUserId = conflict.AssignedToUserId,
            ResolvedDate = conflict.ResolvedDate,
            ResolvedByUserId = conflict.ResolvedByUserId,
            EscalatedDate = conflict.EscalatedDate,
            EscalatedByUserId = conflict.EscalatedByUserId,
            ResolutionReason = conflict.ResolutionReason,
            ResolutionNotes = conflict.ResolutionNotes,
            MergedEntityId = conflict.MergedEntityId,
            DiscardedEntityId = conflict.DiscardedEntityId,
            MergeMapping = conflict.MergeMapping,
            EscalationReason = conflict.EscalationReason,
            ReviewAttemptCount = conflict.ReviewAttemptCount,
            ReviewHistory = conflict.ReviewHistory,
            ImportPackageId = conflict.ImportPackageId,
            TargetResolutionHours = conflict.TargetResolutionHours,
            ElapsedTime = conflict.GetElapsedTime(),
            CreatedAtUtc = conflict.CreatedAtUtc,
            LastModifiedAtUtc = conflict.LastModifiedAtUtc
        };
    }
}
