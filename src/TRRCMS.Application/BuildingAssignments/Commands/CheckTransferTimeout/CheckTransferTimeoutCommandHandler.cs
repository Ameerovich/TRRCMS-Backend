using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Commands.CheckTransferTimeout;

/// <summary>
/// Handles <see cref="CheckTransferTimeoutCommand"/> — UC-012 S11.
///
/// Finds all InProgress assignments that have exceeded the timeout threshold
/// and marks them as Failed so the Data Manager can retry.
/// </summary>
public sealed class CheckTransferTimeoutCommandHandler
    : IRequestHandler<CheckTransferTimeoutCommand, TransferTimeoutCheckResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ILogger<CheckTransferTimeoutCommandHandler> _logger;

    public CheckTransferTimeoutCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ILogger<CheckTransferTimeoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransferTimeoutCheckResult> Handle(
        CheckTransferTimeoutCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Authenticate current user ─────────────────────────────────────────
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        // ── 2. Load InProgress assignments ───────────────────────────────────────
        var inProgressAssignments = await _unitOfWork.BuildingAssignments
            .GetInProgressByFieldCollectorAsync(request.FieldCollectorId, cancellationToken);

        _logger.LogInformation(
            "Checking transfer timeout ({TimeoutMinutes} min) for {Count} InProgress assignment(s). FieldCollector filter: {FieldCollectorId}",
            request.TimeoutMinutes, inProgressAssignments.Count,
            request.FieldCollectorId?.ToString() ?? "all");

        // ── 3. Check each assignment against the timeout threshold ───────────────
        var threshold = DateTime.UtcNow.AddMinutes(-request.TimeoutMinutes);
        var timedOutIds = new List<Guid>();
        int stillInProgressCount = 0;

        foreach (var assignment in inProgressAssignments)
        {
            // An assignment is timed out if its last transfer attempt was before the threshold,
            // or if LastTransferAttemptDate is null (should not happen, but defensive).
            if (assignment.LastTransferAttemptDate is null ||
                assignment.LastTransferAttemptDate.Value < threshold)
            {
                assignment.MarkTransferFailed(
                    $"Transfer timeout: no acknowledgement received within {request.TimeoutMinutes} minutes.",
                    currentUserId);

                await _unitOfWork.BuildingAssignments.UpdateAsync(assignment, cancellationToken);
                timedOutIds.Add(assignment.Id);
            }
            else
            {
                stillInProgressCount++;
            }
        }

        // ── 4. Persist all changes atomically ────────────────────────────────────
        if (timedOutIds.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ── 5. Audit log ─────────────────────────────────────────────────────────
        if (timedOutIds.Count > 0)
        {
            await _auditService.LogActionAsync(
                actionType: AuditActionType.StatusChange,
                actionDescription: $"Transfer timeout check: {timedOutIds.Count} assignment(s) timed out after {request.TimeoutMinutes} minutes. {stillInProgressCount} still in progress.",
                entityType: "BuildingAssignment",
                entityIdentifier: $"Batch: {timedOutIds.Count} timed out",
                newValues: System.Text.Json.JsonSerializer.Serialize(new
                {
                    TimedOutCount = timedOutIds.Count,
                    StillInProgressCount = stillInProgressCount,
                    TimeoutMinutes = request.TimeoutMinutes,
                    NewStatus = nameof(TransferStatus.Failed)
                }),
                changedFields: "TransferStatus",
                cancellationToken: cancellationToken);
        }

        // ── 6. Build result ──────────────────────────────────────────────────────
        var message = timedOutIds.Count == 0
            ? stillInProgressCount == 0
                ? "No InProgress assignments found."
                : $"All {stillInProgressCount} InProgress assignment(s) are within the timeout threshold."
            : $"{timedOutIds.Count} assignment(s) timed out and marked as Failed. {stillInProgressCount} still in progress.";

        return new TransferTimeoutCheckResult(
            TimedOutCount: timedOutIds.Count,
            TimedOutAssignmentIds: timedOutIds,
            StillInProgressCount: stillInProgressCount,
            Message: message);
    }
}
