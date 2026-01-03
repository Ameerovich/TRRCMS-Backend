using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Buildings.Commands.CreateBuilding;

public class CreateBuildingCommandHandler : IRequestHandler<CreateBuildingCommand, Guid>
{
    private readonly IBuildingRepository _buildingRepository;

    public CreateBuildingCommandHandler(IBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public async Task<Guid> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
    {
        // TODO: Get actual user ID from authentication context
        var userId = Guid.NewGuid(); // Temporary - replace with actual user

        // Check if building ID already exists
        var buildingId = $"{request.GovernorateCode}{request.DistrictCode}{request.SubDistrictCode}" +
                        $"{request.CommunityCode}{request.NeighborhoodCode}{request.BuildingNumber}";

        var existingBuilding = await _buildingRepository.GetByBuildingIdAsync(buildingId, cancellationToken);
        if (existingBuilding != null)
        {
            throw new InvalidOperationException($"Building with ID {buildingId} already exists.");
        }

        // Create building entity
        var building = Building.Create(
            request.GovernorateCode,
            request.DistrictCode,
            request.SubDistrictCode,
            request.CommunityCode,
            request.NeighborhoodCode,
            request.BuildingNumber,
            request.GovernorateName,
            request.DistrictName,
            request.SubDistrictName,
            request.CommunityName,
            request.NeighborhoodName,
            request.BuildingType,
            userId
        );

        // Set coordinates if provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            building.SetCoordinates(request.Latitude.Value, request.Longitude.Value, userId);
        }

        // Save to database
        await _buildingRepository.AddAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        return building.Id;
    }
}