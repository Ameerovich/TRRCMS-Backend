using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.ToggleBuildingLock;

/// <summary>
/// Handler for ToggleBuildingLockCommand.
/// Locks or unlocks a building to control import pipeline updates.
/// </summary>
public class ToggleBuildingLockCommandHandler : IRequestHandler<ToggleBuildingLockCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public ToggleBuildingLockCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Unit> Handle(ToggleBuildingLockCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var building = await _unitOfWork.Buildings.GetByIdAsync(request.BuildingId, cancellationToken)
            ?? throw new NotFoundException($"Building with ID {request.BuildingId} not found");

        var oldValue = building.IsLocked;

        if (request.IsLocked)
            building.Lock(currentUserId);
        else
            building.Unlock(currentUserId);

        await _unitOfWork.Buildings.UpdateAsync(building, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Building {building.BuildingId} {(request.IsLocked ? "locked" : "unlocked")}",
            entityType: "Building",
            entityId: building.Id,
            entityIdentifier: building.BuildingId,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { IsLocked = oldValue }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new { IsLocked = building.IsLocked }),
            changedFields: "IsLocked",
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
