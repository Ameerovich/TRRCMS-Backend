namespace TRRCMS.Application.BuildingAssignments.Dtos;

/// <summary>
/// Result of <see cref="Commands.InitiateTransfer.InitiateTransferCommand"/>.
/// UC-012: S08 — Initiate building transfer to tablet.
/// </summary>
public sealed record InitiateTransferResult(
    int InitiatedCount,
    int FailedCount,
    IReadOnlyList<Guid> FailedAssignmentIds,
    IReadOnlyList<string> FailedReasons,
    bool TabletConnected,
    string Message
);

/// <summary>
/// Result of <see cref="Commands.CheckTransferTimeout.CheckTransferTimeoutCommand"/>.
/// UC-012: S11 — Transfer timeout/failure handling.
/// </summary>
public sealed record TransferTimeoutCheckResult(
    int TimedOutCount,
    IReadOnlyList<Guid> TimedOutAssignmentIds,
    int StillInProgressCount,
    string Message
);

/// <summary>
/// Result of <see cref="Commands.RetryTransfer.RetryTransferCommand"/>.
/// UC-012: S12 — Retry failed transfer.
/// </summary>
public sealed record RetryTransferResult(
    int RetriedCount,
    int FailedCount,
    IReadOnlyList<Guid> FailedAssignmentIds,
    IReadOnlyList<string> FailedReasons,
    string Message
);
