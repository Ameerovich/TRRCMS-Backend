using MediatR;
using TRRCMS.Application.Sync.DTOs;

namespace TRRCMS.Application.Sync.Queries.GetSyncAssignments;

/// <summary>
/// Query that assembles the full synchronisation download payload for the
/// authenticated field collector's tablet (Sync Protocol Step 3).
///
/// The resulting <see cref="SyncAssignmentPayloadDto"/> contains:
/// <list type="bullet">
///   <item>All active building assignments whose <c>TransferStatus</c> is
///         <c>Pending</c> or <c>Failed</c>, with nested building details and
///         property units.</item>
///   <item>A snapshot of every active controlled vocabulary so the tablet can
///         operate entirely offline during field collection.</item>
/// </list>
///
/// The query is dispatched by <c>SyncController.GetAssignments</c> and handled
/// by <see cref="GetSyncAssignmentsQueryHandler"/>.
///
/// Sync Protocol Step 3 – GET /api/v1/sync/assignments.
/// FSD: FR-D-5 (Sync Package Contents), FR-V-1 (Vocabulary Delivery).
/// UC-012: Assign Buildings to Field Collectors.
/// </summary>
/// <param name="SyncSessionId">
/// ID of the active <c>SyncSession</c> opened in Step 1.
/// The handler records the download count and vocabulary versions on this session.
/// </param>
/// <param name="ModifiedSinceUtc">
/// Optional incremental-sync filter.  When provided, only assignments created or
/// modified after this timestamp are included, reducing payload size for tablets
/// that have already downloaded a prior batch.  Pass <c>null</c> for a full sync.
/// </param>
public sealed record GetSyncAssignmentsQuery(
    Guid SyncSessionId,
    DateTime? ModifiedSinceUtc = null
) : IRequest<SyncAssignmentPayloadDto>;
