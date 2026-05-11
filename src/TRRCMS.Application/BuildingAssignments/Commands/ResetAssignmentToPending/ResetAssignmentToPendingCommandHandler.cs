using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Commands.ResetAssignmentToPending;

public class ResetAssignmentToPendingCommandHandler
    : IRequestHandler<ResetAssignmentToPendingCommand, ResetAssignmentToPendingResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ILogger<ResetAssignmentToPendingCommandHandler> _logger;

    public ResetAssignmentToPendingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ILogger<ResetAssignmentToPendingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ResetAssignmentToPendingResult> Handle(
        ResetAssignmentToPendingCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var assignment = await _unitOfWork.BuildingAssignments.GetByIdAsync(request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException($"Assignment with ID {request.AssignmentId} not found");

        var building = await _unitOfWork.Buildings.GetByIdAsync(assignment.BuildingId, cancellationToken);
        var fieldCollector = await _unitOfWork.Users.GetByIdAsync(assignment.FieldCollectorId, cancellationToken);

        var buildingCode = building?.BuildingId ?? "Unknown";
        var fieldCollectorName = fieldCollector?.FullNameArabic ?? "Unknown";
        var previousStatus = assignment.TransferStatus.ToString();

        var oldValues = new
        {
            assignment.TransferStatus,
            assignment.IsActive,
            assignment.TransferErrorMessage,
            assignment.TransferRetryCount,
            assignment.TransferredToTabletDate
        };

        assignment.ResetTransferToPending(request.Reason, currentUserId);

        await _unitOfWork.BuildingAssignments.UpdateAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Reset assignment {AssignmentId} (building {BuildingCode}) from {PreviousStatus} to Pending. Reason: {Reason}",
            request.AssignmentId, buildingCode, previousStatus, request.Reason ?? "not specified");

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Reset transfer status to Pending for building {buildingCode} (previously {previousStatus})",
            entityType: "BuildingAssignment",
            entityId: assignment.Id,
            entityIdentifier: buildingCode,
            oldValues: System.Text.Json.JsonSerializer.Serialize(oldValues),
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                assignment.TransferStatus,
                assignment.IsActive,
                Reason = request.Reason
            }),
            changedFields: "TransferStatus, IsActive, TransferErrorMessage, TransferRetryCount, TransferredToTabletDate",
            cancellationToken: cancellationToken);

        return new ResetAssignmentToPendingResult
        {
            Success = true,
            Message = $"Assignment for building {buildingCode} has been reset to Pending and will be transferred on next sync",
            AssignmentId = assignment.Id,
            BuildingCode = buildingCode,
            FieldCollectorName = fieldCollectorName,
            PreviousStatus = previousStatus
        };
    }
}
