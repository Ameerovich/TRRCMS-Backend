using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Commands.UnassignBuilding;

/// <summary>
/// Handler for UnassignBuildingCommand
/// </summary>
public class UnassignBuildingCommandHandler : IRequestHandler<UnassignBuildingCommand, UnassignBuildingResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ILogger<UnassignBuildingCommandHandler> _logger;

    public UnassignBuildingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ILogger<UnassignBuildingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UnassignBuildingResult> Handle(UnassignBuildingCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get assignment
        var assignment = await _unitOfWork.BuildingAssignments.GetByIdAsync(request.AssignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new NotFoundException($"Assignment with ID {request.AssignmentId} not found");
        }

        // Get building and field collector details for response and logging
        var building = await _unitOfWork.Buildings.GetByIdAsync(assignment.BuildingId, cancellationToken);
        var fieldCollector = await _unitOfWork.Users.GetByIdAsync(assignment.FieldCollectorId, cancellationToken);

        var buildingCode = building?.BuildingId ?? "Unknown";
        var fieldCollectorName = fieldCollector?.FullNameArabic ?? "Unknown";

        // Check if assignment can be cancelled
        if (!assignment.IsActive)
        {
            throw new ValidationException($"Assignment is already inactive and cannot be cancelled");
        }

        if (assignment.TransferStatus == TransferStatus.Synchronized)
        {
            throw new ValidationException(
                $"Cannot cancel assignment that has already been synchronized. " +
                $"Data has been collected for this building.");
        }

        // Store old values for audit
        var oldValues = new
        {
            assignment.IsActive,
            assignment.TransferStatus,
            assignment.FieldCollectorId,
            FieldCollectorName = fieldCollectorName
        };

        // Cancel the assignment
        assignment.Cancel(request.CancellationReason, currentUserId);

        await _unitOfWork.BuildingAssignments.UpdateAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cancelled assignment {AssignmentId} for building {BuildingCode}. Reason: {Reason}",
            request.AssignmentId, buildingCode, request.CancellationReason);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Cancelled building assignment for {buildingCode} (was assigned to {fieldCollectorName})",
            entityType: "BuildingAssignment",
            entityId: assignment.Id,
            entityIdentifier: buildingCode,
            oldValues: System.Text.Json.JsonSerializer.Serialize(oldValues),
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                assignment.IsActive,
                assignment.TransferStatus,
                CancellationReason = request.CancellationReason
            }),
            changedFields: "IsActive, TransferStatus, AssignmentNotes",
            cancellationToken: cancellationToken
        );

        return new UnassignBuildingResult
        {
            Success = true,
            Message = $"Successfully cancelled assignment for building {buildingCode}",
            AssignmentId = assignment.Id,
            BuildingCode = buildingCode,
            FieldCollectorName = fieldCollectorName
        };
    }
}
