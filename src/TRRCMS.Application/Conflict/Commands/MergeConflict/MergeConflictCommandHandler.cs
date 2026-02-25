using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Commands.MergeConflict;

/// <summary>
/// Handler for <see cref="MergeConflictCommand"/>.
///
/// Orchestrates the duplicate merge workflow:
/// 1. Validates conflict exists and is PendingReview.
/// 2. Validates MasterEntityId is one of the two conflict entities.
/// 3. Records the review attempt for audit trail.
/// 4. Delegates to the appropriate IMergeService (Person or Property).
/// 5. Updates the ConflictResolution entity with merge results.
/// 6. If all conflicts for the import package are resolved, transitions package to ReadyToCommit.
/// 7. Logs the merge action via IAuditService.
///
/// UC-007 S06–S07: Apply Property Merge Rules, Update Records and Log Resolution.
/// UC-008 S06–S07: Apply Person Merge Rules, Update Linked Relationships and Log Resolution.
/// </summary>
public class MergeConflictCommandHandler
    : IRequestHandler<MergeConflictCommand, ConflictDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMergeService _personMergeService;
    private readonly IMergeService _propertyMergeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public MergeConflictCommandHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<IMergeService> mergeServices,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService
            ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService
            ?? throw new ArgumentNullException(nameof(auditService));

        var services = mergeServices.ToList();
        _personMergeService = services.FirstOrDefault(s => s.EntityType == "Person")
            ?? throw new InvalidOperationException("PersonMergeService not registered.");
        _propertyMergeService = services.FirstOrDefault(s => s.EntityType == "PropertyUnit")
            ?? throw new InvalidOperationException("PropertyMergeService not registered.");
    }

    public async Task<ConflictDetailDto> Handle(
        MergeConflictCommand request,
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
                "and cannot be merged. Only PendingReview conflicts can be merged.");
        }

        // 2. Determine master and discarded entities
        var masterEntityId = request.MasterEntityId ?? conflict.FirstEntityId;

        if (masterEntityId != conflict.FirstEntityId &&
            masterEntityId != conflict.SecondEntityId)
        {
            throw new ConflictException(
                $"MasterEntityId '{masterEntityId}' must be either FirstEntityId " +
                $"'{conflict.FirstEntityId}' or SecondEntityId '{conflict.SecondEntityId}'.");
        }

        var discardedEntityId = masterEntityId == conflict.FirstEntityId
            ? conflict.SecondEntityId
            : conflict.FirstEntityId;

        // 3. Record review attempt
        conflict.RecordReviewAttempt(
            $"Merge: master={masterEntityId}, discarded={discardedEntityId} — {request.Reason}",
            userId);

        // 4. Execute merge via appropriate service
        var mergeService = conflict.EntityType switch
        {
            "Person" => _personMergeService,
            "PropertyUnit" => _propertyMergeService,
            _ => throw new InvalidOperationException(
                $"No merge service registered for entity type '{conflict.EntityType}'.")
        };

        var mergeResult = await mergeService.MergeAsync(
            masterEntityId, discardedEntityId, conflict.ImportPackageId, cancellationToken);

        if (!mergeResult.Success)
        {
            throw new ConflictException(
                $"Merge failed for conflict {conflict.ConflictNumber}: {mergeResult.ErrorMessage}");
        }

        // 5. Update conflict via domain method
        conflict.Resolve(
            action: ConflictResolutionAction.Merge,
            resolutionReason: request.Reason,
            resolutionNotes: request.Notes,
            mergedEntityId: mergeResult.MasterEntityId,
            discardedEntityId: mergeResult.DiscardedEntityId,
            mergeMappingJson: mergeResult.MergeMappingJson,
            resolvedByUserId: userId,
            modifiedByUserId: userId);

        await _unitOfWork.ConflictResolutions.UpdateAsync(conflict, cancellationToken);

        // 6. Check if all conflicts for package are resolved → transition package
        if (conflict.ImportPackageId.HasValue)
        {
            await TryTransitionPackageToReadyAsync(
                conflict.ImportPackageId.Value, userId, cancellationToken);
        }

        // Atomic save: conflict resolution + package transition + merge changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Audit log
        var entityTypeLabel = conflict.EntityType == "Person" ? "person" : "property unit";
        await _auditService.LogActionAsync(
            AuditActionType.Merge,
            $"Conflict {conflict.ConflictNumber}: merged {entityTypeLabel} records. " +
            $"Master: {mergeResult.MasterEntityId}, Discarded: {mergeResult.DiscardedEntityId}. " +
            $"References updated: {mergeResult.ReferencesUpdated}.",
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
