using MediatR;
using TRRCMS.Application.Sync.DTOs;

namespace TRRCMS.Application.Sync.Commands.AcknowledgeSyncAssignments;

/// <summary>
/// Command sent by the tablet after it has stored the downloaded assignments
/// locally and is ready to confirm receipt (Sync Protocol Step 4).
///
/// The handler transitions each acknowledged <c>BuildingAssignment</c> from
/// <c>Pending/Failed</c> to <c>Transferred</c> and updates the
/// <c>SyncSession</c> acknowledgement counter.
///
/// The operation is idempotent: calling it a second time with the same
/// assignment IDs is safe — <c>MarkAsTransferred</c> on the domain entity
/// is a no-op when the status is already <c>Transferred</c>.
///
/// Dispatched by <c>SyncController.AcknowledgeAssignments</c> and handled by
/// <see cref="AcknowledgeSyncAssignmentsCommandHandler"/>.
///
/// Sync Protocol Step 4 – POST /api/v1/sync/assignments/ack.
/// FSD: FR-D-6 (Transfer Acknowledgement).
/// </summary>
/// <param name="SyncSessionId">
/// ID of the <c>SyncSession</c> opened in Step 1.
/// The handler validates that the session belongs to the current user and
/// records the acknowledgement count on it.
/// </param>
/// <param name="AssignmentIds">
/// List of <c>BuildingAssignment</c> surrogate IDs (returned in
/// <see cref="SyncBuildingDto.AssignmentId"/> during Step 3) that the tablet
/// confirms it has received and stored locally.
/// Must contain at least one ID.
/// </param>
public sealed record AcknowledgeSyncAssignmentsCommand(
    Guid SyncSessionId,
    IReadOnlyList<Guid> AssignmentIds
) : IRequest<SyncAckResultDto>;
