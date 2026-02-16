using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Commands.EscalateConflict;

/// <summary>
/// Handler for <see cref="EscalateConflictCommand"/>.
/// 
/// 1. Validates conflict exists and is in PendingReview status.
/// 2. Records review attempt.
/// 3. Escalates via domain method (sets IsEscalated=true, Priority=High).
/// 4. Logs escalation via IAuditService.
/// </summary>
public class EscalateConflictCommandHandler
    : IRequestHandler<EscalateConflictCommand, ConflictDetailDto>
{
    private readonly IConflictResolutionRepository _conflictRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public EscalateConflictCommandHandler(
        IConflictResolutionRepository conflictRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _conflictRepository = conflictRepository
            ?? throw new ArgumentNullException(nameof(conflictRepository));
        _currentUserService = currentUserService
            ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService
            ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<ConflictDetailDto> Handle(
        EscalateConflictCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        var conflict = await _conflictRepository.GetByIdAsync(request.ConflictId, cancellationToken)
            ?? throw new NotFoundException(
                $"Conflict with ID {request.ConflictId} not found.");

        if (conflict.Status != "PendingReview")
        {
            throw new ConflictException(
                $"Conflict {conflict.ConflictNumber} is in '{conflict.Status}' status " +
                "and cannot be escalated. Only PendingReview conflicts can be escalated.");
        }

        if (conflict.IsEscalated)
        {
            throw new ConflictException(
                $"Conflict {conflict.ConflictNumber} has already been escalated.");
        }

        // Record review attempt before escalation
        conflict.RecordReviewAttempt($"Escalated: {request.Reason}", userId);

        // Escalate via domain method
        conflict.Escalate(
            escalationReason: request.Reason,
            escalatedByUserId: userId,
            modifiedByUserId: userId);

        await _conflictRepository.UpdateAsync(conflict, cancellationToken);
        await _conflictRepository.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogActionAsync(
            AuditActionType.Update,
            $"Conflict {conflict.ConflictNumber} escalated to senior review. Reason: {request.Reason}",
            entityType: "ConflictResolution",
            entityId: conflict.Id,
            entityIdentifier: conflict.ConflictNumber,
            cancellationToken: cancellationToken);

        return MapToDetailDto(conflict);
    }

    private static ConflictDetailDto MapToDetailDto(Domain.Entities.ConflictResolution c)
    {
        return new ConflictDetailDto
        {
            Id = c.Id,
            ConflictNumber = c.ConflictNumber,
            ConflictType = c.ConflictType,
            EntityType = c.EntityType,
            ConflictDescription = c.ConflictDescription,
            FirstEntityId = c.FirstEntityId,
            SecondEntityId = c.SecondEntityId,
            FirstEntityIdentifier = c.FirstEntityIdentifier,
            SecondEntityIdentifier = c.SecondEntityIdentifier,
            SimilarityScore = c.SimilarityScore,
            ConfidenceLevel = c.ConfidenceLevel,
            MatchingCriteria = c.MatchingCriteria,
            DataComparison = c.DataComparison,
            Status = c.Status,
            ResolutionAction = c.ResolutionAction,
            Priority = c.Priority,
            IsEscalated = c.IsEscalated,
            IsOverdue = c.IsOverdue,
            IsAutoDetected = c.IsAutoDetected,
            IsAutoResolved = c.IsAutoResolved,
            AutoResolutionRule = c.AutoResolutionRule,
            DetectedDate = c.DetectedDate,
            DetectedByUserId = c.DetectedByUserId,
            AssignedDate = c.AssignedDate,
            AssignedToUserId = c.AssignedToUserId,
            ResolvedDate = c.ResolvedDate,
            ResolvedByUserId = c.ResolvedByUserId,
            EscalatedDate = c.EscalatedDate,
            EscalatedByUserId = c.EscalatedByUserId,
            ResolutionReason = c.ResolutionReason,
            ResolutionNotes = c.ResolutionNotes,
            MergedEntityId = c.MergedEntityId,
            DiscardedEntityId = c.DiscardedEntityId,
            MergeMapping = c.MergeMapping,
            EscalationReason = c.EscalationReason,
            ReviewAttemptCount = c.ReviewAttemptCount,
            ReviewHistory = c.ReviewHistory,
            ImportPackageId = c.ImportPackageId,
            TargetResolutionHours = c.TargetResolutionHours,
            ElapsedTime = c.GetElapsedTime(),
            CreatedAtUtc = c.CreatedAtUtc,
            LastModifiedAtUtc = c.LastModifiedAtUtc
        };
    }
}
