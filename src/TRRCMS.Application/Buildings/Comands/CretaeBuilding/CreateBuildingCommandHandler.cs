using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Buildings.Commands.CreateBuilding;

public class CreateBuildingCommandHandler : IRequestHandler<CreateBuildingCommand, Guid>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateBuildingCommandHandler(
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService)
    {
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
    {
        // Get current user ID
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

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

        // Set building details if provided (address, landmark, notes, floors, year)
        if (request.NumberOfFloors.HasValue ||
            request.YearOfConstruction.HasValue ||
            !string.IsNullOrWhiteSpace(request.Address) ||
            !string.IsNullOrWhiteSpace(request.Landmark) ||
            !string.IsNullOrWhiteSpace(request.Notes))
        {
            building.UpdateDetails(
                numberOfFloors: request.NumberOfFloors,
                yearOfConstruction: request.YearOfConstruction,
                address: request.Address,
                landmark: request.Landmark,
                notes: request.Notes,
                modifiedByUserId: userId
            );
        }

        // Save to database
        await _buildingRepository.AddAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        return building.Id;
    }
}