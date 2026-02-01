using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Commands.DeleteBuilding;

/// <summary>
/// Handler for DeleteBuildingCommand
/// Performs soft delete - sets IsDeleted flag
/// </summary>
public class DeleteBuildingCommandHandler : IRequestHandler<DeleteBuildingCommand, bool>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteBuildingCommandHandler(
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService)
    {
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteBuildingCommand request, CancellationToken cancellationToken)
    {
        // Get the building
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId, cancellationToken);

        if (building == null)
        {
            throw new NotFoundException($"Building with ID {request.BuildingId} not found.");
        }

        // Get current user ID
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Soft delete the building
        building.MarkAsDeleted(userId);

        // Save changes
        await _buildingRepository.UpdateAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}