using MediatR;

namespace TRRCMS.Application.Buildings.Commands.ToggleBuildingLock;

/// <summary>
/// Command to lock or unlock a building.
/// Locked buildings are not updated by the import pipeline.
/// </summary>
public record ToggleBuildingLockCommand(Guid BuildingId, bool IsLocked) : IRequest<Unit>;
