using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Sync.DTOs;

namespace TRRCMS.Application.Sync.Commands.AcknowledgeSyncAssignments;

/// <summary>
/// Handles <see cref="AcknowledgeSyncAssignmentsCommand"/> — Sync Protocol Step 4.
///
/// Execution flow:
/// <list type="number">
///   <item>Validate the current user's identity.</item>
///   <item>Resolve and authorise the <c>SyncSession</c>.</item>
///   <item>For each submitted assignment ID:
///     <list type="bullet">
///       <item>Load the <c>BuildingAssignment</c> entity.</item>
///       <item>Verify ownership (FieldCollectorId == currentUserId).</item>
///       <item>Call <c>assignment.MarkAsTransferred()</c> (domain method).</item>
///       <item>Persist the updated entity via the repository.</item>
///     </list>
///   </item>
///   <item>Record the total acknowledged count on the <c>SyncSession</c>.</item>
///   <item>Persist all changes in a single <c>SaveChangesAsync</c> call.</item>
///   <item>Return a <see cref="SyncAckResultDto"/> with counts and failed IDs.</item>
/// </list>
///
/// Security: only the field collector who owns the session may acknowledge
/// assignments, and only assignments assigned to that collector are accepted.
/// This prevents cross-collector data manipulation.
/// </summary>
public sealed class AcknowledgeSyncAssignmentsCommandHandler
    : IRequestHandler<AcknowledgeSyncAssignmentsCommand, SyncAckResultDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IBuildingAssignmentRepository _assignmentRepo;
    private readonly ICurrentUserService _currentUser;

    public AcknowledgeSyncAssignmentsCommandHandler(
        IUnitOfWork uow,
        IBuildingAssignmentRepository assignmentRepo,
        ICurrentUserService currentUser)
    {
        _uow = uow;
        _assignmentRepo = assignmentRepo;
        _currentUser = currentUser;
    }

    public async Task<SyncAckResultDto> Handle(
        AcknowledgeSyncAssignmentsCommand request,
        CancellationToken ct)
    {
        // ── 1. Resolve authenticated user ──────────────────────────────────────────
        var currentUserId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        // ── 2. Resolve and validate the sync session ───────────────────────────────
        var session = await _uow.SyncSessions.GetByIdAsync(request.SyncSessionId, ct)
            ?? throw new InvalidOperationException(
                $"Sync session '{request.SyncSessionId}' not found.");

        // Ensure the requesting user owns this session.
        if (session.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException(
                "The current user is not the field collector for this sync session.");

        // ── 3. Process each assignment ID ──────────────────────────────────────────
        var failedIds = new List<Guid>();
        int acknowledgedCount = 0;

        foreach (var assignmentId in request.AssignmentIds)
        {
            var assignment = await _assignmentRepo.GetByIdAsync(assignmentId, ct);

            if (assignment is null)
            {
                // Assignment not found — record as failed but continue processing
                // the remaining IDs so the tablet gets a full picture.
                failedIds.Add(assignmentId);
                continue;
            }

            // Security guard: the assignment must belong to the authenticated
            // field collector.  Silently skip (and flag) anything that does not.
            if (assignment.FieldCollectorId != currentUserId)
            {
                failedIds.Add(assignmentId);
                continue;
            }

            // Domain method — idempotent: calling MarkAsTransferred twice is safe;
            // the entity simply updates the timestamp and audit fields again.
            assignment.MarkAsTransferred(currentUserId);
            await _assignmentRepo.UpdateAsync(assignment, ct);

            acknowledgedCount++;
        }

        // ── 4. Record the acknowledgement on the session ───────────────────────────
        session.RecordAcknowledgment(acknowledgedCount);
        await _uow.SyncSessions.UpdateAsync(session, ct);

        // ── 5. Persist all changes atomically ─────────────────────────────────────
        await _uow.SaveChangesAsync(ct);

        // ── 6. Build and return the result ─────────────────────────────────────────
        var message = failedIds.Count == 0
            ? $"All {acknowledgedCount} assignment(s) acknowledged successfully."
            : $"{acknowledgedCount} acknowledged; {failedIds.Count} failed (not found or not owned by caller).";

        return new SyncAckResultDto(
            AcknowledgedCount: acknowledgedCount,
            FailedCount: failedIds.Count,
            FailedAssignmentIds: failedIds,
            Message: message);
    }
}
