using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;

namespace TRRCMS.Application.BuildingAssignments.Commands.RetryTransfer;

/// <summary>
/// Command to retry failed transfers by resetting assignments from Failed
/// back to Pending, making them eligible for the next sync download.
/// UC-012: S12 — Retry failed transfer.
/// </summary>
public sealed record RetryTransferCommand(
    IReadOnlyList<Guid> AssignmentIds
) : IRequest<RetryTransferResult>;
