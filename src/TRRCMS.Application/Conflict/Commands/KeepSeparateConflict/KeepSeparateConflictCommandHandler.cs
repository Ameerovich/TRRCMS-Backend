using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Commands.KeepSeparateConflict;

/// <summary>
/// Handler for <see cref="KeepSeparateConflictCommand"/>.
///
/// Marks a conflict as reviewed with a "keep separate" decision:
/// 1. Validates conflict exists and is PendingReview.
/// 2. Records the review attempt for audit trail.
/// 3. Resolves via domain method with KeepBoth action (no merge, no entity changes).
/// 4. If all conflicts for the import package are resolved, transitions package to ReadyToCommit.
/// 5. Logs the decision via IAuditService.
///
/// Per UC rules: keep-separate decisions prevent the same group from being
/// re-surfaced as a duplicate unless detection rules or keys change.
/// </summary>
public class KeepSeparateConflictCommandHandler
    : IRequestHandler<KeepSeparateConflictCommand, ConflictDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public KeepSeparateConflictCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService
            ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService
            ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<ConflictDetailDto> Handle(
        KeepSeparateConflictCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        // 1. Load and validate conflict
        var conflict = await _unitOfWork.ConflictResolutions
                .GetByIdAsync(request.ConflictId, cancellationToken)
            ?? throw new NotFoundException(
                $"Conflict with ID {request.ConflictId} not found.");

        if (conflict.Status != "PendingReview")
        {
            throw new ConflictException(
                $"Conflict {conflict.ConflictNumber} is in '{conflict.Status}' status " +
                "and cannot be updated. Only PendingReview conflicts can be resolved.");
        }

        // 2. Record review attempt
        conflict.RecordReviewAttempt(
            $"Keep-Separate: {request.Reason}", userId);

        // 3. Resolve as KeepBoth — no merge, no entity changes
        conflict.Resolve(
            action: ConflictResolutionAction.KeepBoth,
            resolutionReason: request.Reason,
            resolutionNotes: request.Notes,
            mergedEntityId: null,
            discardedEntityId: null,
            mergeMappingJson: null,
            resolvedByUserId: userId,
            modifiedByUserId: userId);

        await _unitOfWork.ConflictResolutions.UpdateAsync(conflict, cancellationToken);

        // 4. Check if all conflicts for package are resolved → transition package
        if (conflict.ImportPackageId.HasValue)
        {
            await TryTransitionPackageToReadyAsync(
                conflict.ImportPackageId.Value, userId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Audit log
        await _auditService.LogActionAsync(
            AuditActionType.ConflictResolved,
            $"Conflict {conflict.ConflictNumber} resolved as keep-separate. " +
            $"Entities {conflict.FirstEntityIdentifier} and {conflict.SecondEntityIdentifier} " +
            "confirmed as distinct records.",
            entityType: "ConflictResolution",
            entityId: conflict.Id,
            entityIdentifier: conflict.ConflictNumber,
            cancellationToken: cancellationToken);

        return MapToDetailDto(conflict);
    }

    /// <summary>
    /// When all conflicts for a package are resolved, transition it to ReadyToCommit.
    /// </summary>
    private async Task TryTransitionPackageToReadyAsync(
        Guid importPackageId, Guid userId, CancellationToken cancellationToken)
    {
        var allResolved = await _unitOfWork.ConflictResolutions
            .AreAllResolvedForPackageAsync(importPackageId, cancellationToken);

        if (!allResolved) return;

        var package = await _unitOfWork.ImportPackages
            .GetByIdAsync(importPackageId, cancellationToken);

        if (package is null) return;

        if (package.Status == ImportStatus.ReviewingConflicts)
        {
            package.MarkConflictsResolved(userId);
            await _unitOfWork.ImportPackages.UpdateAsync(package, cancellationToken);
        }
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
