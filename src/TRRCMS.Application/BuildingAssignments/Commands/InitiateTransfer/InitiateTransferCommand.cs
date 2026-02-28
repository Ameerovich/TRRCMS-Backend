using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;

namespace TRRCMS.Application.BuildingAssignments.Commands.InitiateTransfer;

/// <summary>
/// Command for Data Manager to explicitly initiate transfer of assignments
/// to a field collector's tablet. Validates tablet connectivity (active sync
/// session) and transitions assignments from Pending to InProgress.
/// UC-012: S08 — Initiate Building Transfer to Tablet.
/// </summary>
public sealed record InitiateTransferCommand(
    Guid FieldCollectorId,
    IReadOnlyList<Guid> AssignmentIds
) : IRequest<InitiateTransferResult>;
