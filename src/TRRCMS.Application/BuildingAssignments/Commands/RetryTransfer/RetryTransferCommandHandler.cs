using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Commands.RetryTransfer;

/// <summary>
/// Handles <see cref="RetryTransferCommand"/> — UC-012 S12.
///
/// Resets each Failed assignment back to Pending so it becomes eligible
/// for the next sync download. The TransferRetryCount is preserved
/// (incremented by MarkTransferFailed, not reset here).
/// </summary>
public sealed class RetryTransferCommandHandler
    : IRequestHandler<RetryTransferCommand, RetryTransferResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ILogger<RetryTransferCommandHandler> _logger;

    public RetryTransferCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ILogger<RetryTransferCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RetryTransferResult> Handle(
        RetryTransferCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Authenticate current user ─────────────────────────────────────────
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        _logger.LogInformation(
            "Retrying transfer for {Count} assignment(s)", request.AssignmentIds.Count);

        // ── 2. Process each assignment ───────────────────────────────────────────
        var failedIds = new List<Guid>();
        var failedReasons = new List<string>();
        int retriedCount = 0;

        foreach (var assignmentId in request.AssignmentIds)
        {
            var assignment = await _unitOfWork.BuildingAssignments
                .GetByIdAsync(assignmentId, cancellationToken);

            if (assignment is null)
            {
                failedIds.Add(assignmentId);
                failedReasons.Add($"Assignment {assignmentId} not found.");
                continue;
            }

            if (!assignment.IsActive)
            {
                failedIds.Add(assignmentId);
                failedReasons.Add($"Assignment {assignmentId} is inactive.");
                continue;
            }

            if (assignment.TransferStatus != TransferStatus.Failed)
            {
                failedIds.Add(assignmentId);
                failedReasons.Add($"Assignment {assignmentId} status is '{assignment.TransferStatus}', expected 'Failed'.");
                continue;
            }

            assignment.ResetForRetry(currentUserId);
            await _unitOfWork.BuildingAssignments.UpdateAsync(assignment, cancellationToken);
            retriedCount++;
        }

        // ── 3. Persist all changes atomically ────────────────────────────────────
        if (retriedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ── 4. Audit log ─────────────────────────────────────────────────────────
        if (retriedCount > 0)
        {
            await _auditService.LogActionAsync(
                actionType: AuditActionType.StatusChange,
                actionDescription: $"Transfer retry: {retriedCount} assignment(s) reset from Failed to Pending.",
                entityType: "BuildingAssignment",
                entityIdentifier: $"Batch: {retriedCount} retried",
                newValues: System.Text.Json.JsonSerializer.Serialize(new
                {
                    RetriedCount = retriedCount,
                    FailedCount = failedIds.Count,
                    NewStatus = nameof(TransferStatus.Pending)
                }),
                changedFields: "TransferStatus,TransferErrorMessage",
                cancellationToken: cancellationToken);
        }

        // ── 5. Build result ──────────────────────────────────────────────────────
        var message = failedIds.Count == 0
            ? $"All {retriedCount} assignment(s) reset to Pending and ready for transfer."
            : $"{retriedCount} assignment(s) retried, {failedIds.Count} failed.";

        return new RetryTransferResult(
            RetriedCount: retriedCount,
            FailedCount: failedIds.Count,
            FailedAssignmentIds: failedIds,
            FailedReasons: failedReasons,
            Message: message);
    }
}
