using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Commands.DeleteBuilding;

/// <summary>
/// Handler for DeleteBuildingCommand
/// Performs soft delete with referential integrity checks
/// Uses repository methods (Clean Architecture)
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

        // ============================================================
        // REFERENTIAL INTEGRITY CHECKS
        // ============================================================

        // Check 1: Property Units linked to this building
        var hasPropertyUnits = await _buildingRepository.HasPropertyUnitsAsync(request.BuildingId, cancellationToken);
        if (hasPropertyUnits)
        {
            throw new ValidationException(
                "Cannot delete building with existing property units. Delete or reassign property units first. | لا يمكن حذف المبنى لوجود وحدات عقارية مرتبطة به.");
        }

        // Check 2: Active Surveys (Draft or Completed)
        var hasActiveSurveys = await _buildingRepository.HasActiveSurveysAsync(request.BuildingId, cancellationToken);
        if (hasActiveSurveys)
        {
            throw new ValidationException(
                "Cannot delete building with active surveys. Finalize or cancel surveys first. | لا يمكن حذف المبنى لوجود مسوحات نشطة.");
        }

        // ============================================================
        // PERFORM SOFT DELETE
        // ============================================================

        var userId = _currentUserService.UserId 
            ?? throw new UnauthorizedAccessException("User not authenticated");

        building.MarkAsDeleted(userId);

        await _buildingRepository.UpdateAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
