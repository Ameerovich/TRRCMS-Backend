using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Conflicts.Commands.ResolveConflict;

/// <summary>
/// Handler for <see cref="ResolveConflictCommand"/>.
///
/// Orchestrates conflict resolution:
/// 1. Validates conflict exists and is in PendingReview status.
/// 2. Records the review attempt.
/// 3. For Merge actions: delegates to the appropriate IMergeService
///    (Person or Property) based on EntityType.
/// 4. Updates ConflictResolution entity via domain method.
/// 5. If this was the last unresolved conflict for an import package,
///    transitions the package to ReadyToCommit.
/// 6. Logs the resolution via IAuditService.
/// </summary>
public class ResolveConflictCommandHandler
    : IRequestHandler<ResolveConflictCommand, ConflictDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMergeService _personMergeService;
    private readonly IMergeService _propertyMergeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public ResolveConflictCommandHandler(
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

        // Resolve named merge services from the DI collection
        var services = mergeServices.ToList();
        _personMergeService = services.FirstOrDefault(s => s.EntityType == "Person")
            ?? throw new InvalidOperationException("PersonMergeService not registered.");
        _propertyMergeService = services.FirstOrDefault(s => s.EntityType == "Building")
            ?? throw new InvalidOperationException("PropertyMergeService not registered.");
    }

    public async Task<ConflictDetailDto> Handle(
        ResolveConflictCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        // 1. Load and validate conflict
        var conflict = await _unitOfWork.ConflictResolutions.GetByIdAsync(request.ConflictId, cancellationToken)
            ?? throw new NotFoundException(
                $"Conflict with ID {request.ConflictId} not found.");

        if (conflict.Status != "PendingReview")
        {
            throw new ConflictException(
                $"Conflict {conflict.ConflictNumber} is in '{conflict.Status}' status " +
                "and cannot be resolved. Only PendingReview conflicts can be resolved.");
        }

        // 2. Record review attempt
        conflict.RecordReviewAttempt(
            $"Resolution: {request.Action} — {request.Reason}", userId);

        // 3. Execute merge if applicable
        Guid? mergedEntityId = null;
        Guid? discardedEntityId = null;
        string? mergeMappingJson = null;

        if (request.Action == ConflictResolutionAction.Merge)
        {
            var mergeResult = await ExecuteMergeAsync(conflict, request, cancellationToken);

            if (!mergeResult.Success)
            {
                throw new ConflictException(
                    $"Merge failed for conflict {conflict.ConflictNumber}: {mergeResult.ErrorMessage}");
            }

            mergedEntityId = mergeResult.MasterEntityId;
            discardedEntityId = mergeResult.DiscardedEntityId;
            mergeMappingJson = mergeResult.MergeMappingJson;
        }
        else if (request.Action == ConflictResolutionAction.KeepFirst)
        {
            mergedEntityId = conflict.FirstEntityId;
            discardedEntityId = conflict.SecondEntityId;
        }
        else if (request.Action == ConflictResolutionAction.KeepSecond)
        {
            mergedEntityId = conflict.SecondEntityId;
            discardedEntityId = conflict.FirstEntityId;
        }

        // 4. Resolve via domain method
        if (request.Action == ConflictResolutionAction.Ignored)
        {
            conflict.Ignore(request.Reason, userId);
        }
        else
        {
            conflict.Resolve(
                action: request.Action,
                resolutionReason: request.Reason,
                resolutionNotes: request.Notes,
                mergedEntityId: mergedEntityId,
                discardedEntityId: discardedEntityId,
                mergeMappingJson: mergeMappingJson,
                resolvedByUserId: userId,
                modifiedByUserId: userId);
        }

        await _unitOfWork.ConflictResolutions.UpdateAsync(conflict, cancellationToken);

        // 5. Check if all conflicts for package are resolved → transition package
        if (conflict.ImportPackageId.HasValue)
        {
            await TryTransitionPackageToReadyAsync(
                conflict.ImportPackageId.Value, userId, cancellationToken);
        }

        // Single atomic save for both conflict resolution and package transition
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Audit log
        await _auditService.LogActionAsync(
            AuditActionType.ConflictResolved,
            $"Conflict {conflict.ConflictNumber} resolved with action '{request.Action}'.",
            entityType: "ConflictResolution",
            entityId: conflict.Id,
            entityIdentifier: conflict.ConflictNumber,
            cancellationToken: cancellationToken);

        // 7. Return updated detail
        return MapToDetailDto(conflict);
    }

    /// <summary>
    /// Delegates to the appropriate merge service based on entity type.
    /// </summary>
    private async Task<MergeResultDto> ExecuteMergeAsync(
        Domain.Entities.ConflictResolution conflict,
        ResolveConflictCommand request,
        CancellationToken cancellationToken)
    {
        var mergeService = conflict.EntityType switch
        {
            "Person" => _personMergeService,
            "Building" or "PropertyUnit" => _propertyMergeService,
            _ => throw new InvalidOperationException(
                $"No merge service registered for entity type '{conflict.EntityType}'.")
        };

        var masterEntityId = request.PreferredMasterEntityId
            ?? conflict.FirstEntityId; // default: first entity is master
        var discardedEntityId = masterEntityId == conflict.FirstEntityId
            ? conflict.SecondEntityId
            : conflict.FirstEntityId;

        return await mergeService.MergeAsync(
            masterEntityId, discardedEntityId, cancellationToken);
    }

    /// <summary>
    /// When all conflicts for a package are resolved, transition it to ReadyToCommit.
    /// Changes are saved atomically with the conflict resolution by the caller.
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
