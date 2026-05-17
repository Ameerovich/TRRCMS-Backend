using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Commands.MergeConflict;

/// <summary>
/// Handler for <see cref="MergeConflictCommand"/>.
///
/// Orchestrates the duplicate merge workflow: validates the conflict,
/// delegates to the appropriate IMergeService, updates the conflict entity,
/// and transitions the import package if all conflicts are resolved.
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

        // Log after merge so the history reflects the actual surviving/discarded IDs,
        // not the pre-flip request IDs (the merge service may swap master/discarded
        // to ensure the production entity always survives in cross-batch merges).
        conflict.RecordReviewAttempt(
            $"Merge: master={mergeResult.MasterEntityId}, discarded={mergeResult.DiscardedEntityId} — {request.Reason}",
            userId);

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

        if (conflict.ImportPackageId.HasValue)
        {
            await TryTransitionPackageToReadyAsync(
                conflict.ImportPackageId.Value, userId, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var entityTypeLabel = conflict.EntityType == "Person" ? "person" : "property unit";
        var masterIdentifier = IdentifierFor(conflict, mergeResult.MasterEntityId);
        var discardedIdentifier = IdentifierFor(conflict, mergeResult.DiscardedEntityId);
        await _auditService.LogActionAsync(
            AuditActionType.Merge,
            $"Conflict {conflict.ConflictNumber}: merged {entityTypeLabel} records. " +
            $"Master: {masterIdentifier}, Discarded: {discardedIdentifier}. " +
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

    /// <summary>
    /// Maps a side ID (master or discarded) back to the human-readable identifier
    /// already stored on the conflict (e.g. "محمد أحمد (1234567890)"). Falls back
    /// to the GUID only when the ID matches neither side — should not happen in
    /// practice but keeps the audit message non-empty if it ever does.
    /// </summary>
    private static string IdentifierFor(Domain.Entities.ConflictResolution conflict, Guid entityId)
    {
        if (entityId == conflict.FirstEntityId && !string.IsNullOrWhiteSpace(conflict.FirstEntityIdentifier))
            return conflict.FirstEntityIdentifier!;
        if (entityId == conflict.SecondEntityId && !string.IsNullOrWhiteSpace(conflict.SecondEntityIdentifier))
            return conflict.SecondEntityIdentifier!;
        return entityId.ToString();
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
