using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Commands.InitiateTransfer;

/// <summary>
/// Handles <see cref="InitiateTransferCommand"/> — UC-012 S08.
///
/// Validates the field collector, checks tablet connectivity (active sync
/// session), and transitions each specified assignment from Pending to InProgress.
/// </summary>
public sealed class InitiateTransferCommandHandler
    : IRequestHandler<InitiateTransferCommand, InitiateTransferResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ILogger<InitiateTransferCommandHandler> _logger;

    public InitiateTransferCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ILogger<InitiateTransferCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InitiateTransferResult> Handle(
        InitiateTransferCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Authenticate current user (Data Manager) ─────────────────────────
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        // ── 2. Validate field collector ──────────────────────────────────────────
        var fieldCollector = await _unitOfWork.Users.GetByIdAsync(
            request.FieldCollectorId, cancellationToken);

        if (fieldCollector is null)
            throw new NotFoundException(
                $"Field collector with ID {request.FieldCollectorId} not found.");

        if (fieldCollector.Role != UserRole.FieldCollector)
            throw new ValidationException(
                $"User {fieldCollector.Username} is not a field collector.");

        if (!fieldCollector.IsActive)
            throw new ValidationException(
                $"Field collector {fieldCollector.Username} is not active.");

        // ── 3. Check tablet connectivity (recent InProgress sync session) ───────
        var recentSessions = await _unitOfWork.SyncSessions.GetByFieldCollectorAsync(
            fieldCollectorId: request.FieldCollectorId,
            fromUtc: DateTime.UtcNow.AddHours(-24),
            status: SyncSessionStatus.InProgress,
            take: 1,
            cancellationToken: cancellationToken);

        var tabletConnected = recentSessions.Count > 0;

        _logger.LogInformation(
            "Initiating transfer of {Count} assignment(s) to field collector {CollectorId} ({CollectorName}). Tablet connected: {TabletConnected}",
            request.AssignmentIds.Count, request.FieldCollectorId,
            fieldCollector.FullNameArabic, tabletConnected);

        // ── 4. Process each assignment ───────────────────────────────────────────
        var failedIds = new List<Guid>();
        var failedReasons = new List<string>();
        int initiatedCount = 0;

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

            if (assignment.FieldCollectorId != request.FieldCollectorId)
            {
                failedIds.Add(assignmentId);
                failedReasons.Add($"Assignment {assignmentId} is not assigned to this field collector.");
                continue;
            }

            if (assignment.TransferStatus != TransferStatus.Pending)
            {
                failedIds.Add(assignmentId);
                failedReasons.Add($"Assignment {assignmentId} status is '{assignment.TransferStatus}', expected 'Pending'.");
                continue;
            }

            if (!assignment.IsActive)
            {
                failedIds.Add(assignmentId);
                failedReasons.Add($"Assignment {assignmentId} is inactive.");
                continue;
            }

            assignment.MarkTransferInProgress(currentUserId);
            await _unitOfWork.BuildingAssignments.UpdateAsync(assignment, cancellationToken);
            initiatedCount++;
        }

        // ── 5. Persist all changes atomically ────────────────────────────────────
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── 6. Audit log ─────────────────────────────────────────────────────────
        await _auditService.LogActionAsync(
            actionType: AuditActionType.StatusChange,
            actionDescription: $"Transfer initiated for {initiatedCount} assignment(s) to field collector {fieldCollector.FullNameArabic} ({fieldCollector.Username}). Tablet connected: {tabletConnected}.",
            entityType: "BuildingAssignment",
            entityId: request.FieldCollectorId,
            entityIdentifier: $"Batch: {initiatedCount} assignments",
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                FieldCollectorId = request.FieldCollectorId,
                InitiatedCount = initiatedCount,
                FailedCount = failedIds.Count,
                TabletConnected = tabletConnected,
                NewStatus = nameof(TransferStatus.InProgress)
            }),
            changedFields: "TransferStatus",
            cancellationToken: cancellationToken);

        // ── 7. Build result ──────────────────────────────────────────────────────
        var message = failedIds.Count == 0
            ? $"Transfer initiated for all {initiatedCount} assignment(s)." +
              (tabletConnected ? " Tablet is connected and will receive the data on next sync."
                               : " Note: Tablet is not currently connected. Assignments will be available when the tablet syncs.")
            : $"{initiatedCount} assignment(s) initiated, {failedIds.Count} failed.";

        return new InitiateTransferResult(
            InitiatedCount: initiatedCount,
            FailedCount: failedIds.Count,
            FailedAssignmentIds: failedIds,
            FailedReasons: failedReasons,
            TabletConnected: tabletConnected,
            Message: message);
    }
}
