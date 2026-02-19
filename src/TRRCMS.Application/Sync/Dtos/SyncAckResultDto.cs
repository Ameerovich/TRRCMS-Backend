namespace TRRCMS.Application.Sync.DTOs;

/// <summary>
/// Result returned by POST /api/v1/sync/assignments/ack.
/// Reports the outcome of the acknowledgement step (Sync Protocol Step 4).
///
/// The tablet sends the list of assignment IDs it has successfully received and
/// stored locally.  The server transitions each acknowledged assignment's
/// <c>TransferStatus</c> from <c>Pending/Failed</c> to <c>Transferred</c>
/// and updates the <c>SyncSession</c> counters.
///
/// FSD: FR-D-6 (Transfer Acknowledgement).
/// </summary>
public sealed record SyncAckResultDto(
    /// <summary>
    /// Number of assignment IDs that were successfully acknowledged.
    /// </summary>
    int AcknowledgedCount,

    /// <summary>
    /// Number of assignment IDs in the request that could not be acknowledged
    /// (e.g., assignment not found, belongs to a different collector,
    /// or already in a terminal state).
    /// </summary>
    int FailedCount,

    /// <summary>
    /// IDs of assignments that could not be acknowledged.
    /// Empty when <see cref="FailedCount"/> is zero.
    /// </summary>
    IReadOnlyList<Guid> FailedAssignmentIds,

    /// <summary>
    /// Human-readable summary of the acknowledgement outcome.
    /// </summary>
    string Message
);
