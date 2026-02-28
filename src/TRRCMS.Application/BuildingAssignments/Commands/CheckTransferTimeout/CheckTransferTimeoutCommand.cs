using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;

namespace TRRCMS.Application.BuildingAssignments.Commands.CheckTransferTimeout;

/// <summary>
/// Command to check for InProgress assignments that have exceeded the
/// transfer timeout threshold and mark them as Failed.
/// Can be invoked manually by Data Manager or by a background scheduler.
/// UC-012: S11 — Transfer timeout/failure handling.
/// </summary>
public sealed record CheckTransferTimeoutCommand(
    /// <summary>
    /// Timeout in minutes. Assignments InProgress for longer than this are marked Failed.
    /// Default: 60 minutes.
    /// </summary>
    int TimeoutMinutes = 60,

    /// <summary>
    /// Optional: restrict check to a specific field collector.
    /// Null = check all field collectors.
    /// </summary>
    Guid? FieldCollectorId = null
) : IRequest<TransferTimeoutCheckResult>;
