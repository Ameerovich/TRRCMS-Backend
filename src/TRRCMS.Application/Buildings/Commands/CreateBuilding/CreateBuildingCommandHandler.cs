using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Buildings.Commands.CreateBuilding;

/// <summary>
/// Handler for CreateBuildingCommand
/// Creates a new building and returns full BuildingDto
/// </summary>
public class CreateBuildingCommandHandler : IRequestHandler<CreateBuildingCommand, BuildingDto>
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

    public async Task<BuildingDto> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
    {
        // Get current user ID
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        // Generate Building ID for duplicate check (stored without dashes)
        var buildingIdCode = $"{request.GovernorateCode}{request.DistrictCode}{request.SubDistrictCode}" +
                             $"{request.CommunityCode}{request.NeighborhoodCode}{request.BuildingNumber}";

        // Check if building ID already exists
        var existingBuilding = await _buildingRepository.GetByBuildingIdAsync(buildingIdCode, cancellationToken);
        if (existingBuilding != null)
        {
            throw new InvalidOperationException($"Building with code {buildingIdCode} already exists.");
        }

        // Create building entity
        // Note: Location names will be empty for now - can be populated from lookup tables later
        var building = Building.Create(
            governorateCode: request.GovernorateCode,
            districtCode: request.DistrictCode,
            subDistrictCode: request.SubDistrictCode,
            communityCode: request.CommunityCode,
            neighborhoodCode: request.NeighborhoodCode,
            buildingNumber: request.BuildingNumber,
            governorateName: string.Empty, // Can be populated from lookup
            districtName: string.Empty,
            subDistrictName: string.Empty,
            communityName: string.Empty,
            neighborhoodName: string.Empty,
            buildingType: request.BuildingType,
            status: request.BuildingStatus,
            createdByUserId: userId
        );

        // Set unit counts
        building.UpdateUnitCounts(
            propertyUnits: request.NumberOfPropertyUnits,
            apartments: request.NumberOfApartments,
            shops: request.NumberOfShops,
            modifiedByUserId: userId
        );

        // Set coordinates if provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            building.SetCoordinates(request.Latitude.Value, request.Longitude.Value, userId);
        }

        // Set geometry if provided
        if (!string.IsNullOrWhiteSpace(request.BuildingGeometryWkt))
        {
            building.SetGeometry(request.BuildingGeometryWkt, userId);
        }

        // Set location description and notes
        if (!string.IsNullOrWhiteSpace(request.LocationDescription) ||
            !string.IsNullOrWhiteSpace(request.Notes))
        {
            building.UpdateLocationInfo(
                locationDescription: request.LocationDescription,
                notes: request.Notes,
                modifiedByUserId: userId
            );
        }

        // Save to database
        await _buildingRepository.AddAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        // Return full DTO
        return MapToDto(building);
    }

    private static BuildingDto MapToDto(Building building)
    {
        return new BuildingDto
        {
            Id = building.Id,
            BuildingId = building.BuildingId,

            // Administrative Codes
            GovernorateCode = building.GovernorateCode,
            DistrictCode = building.DistrictCode,
            SubDistrictCode = building.SubDistrictCode,
            CommunityCode = building.CommunityCode,
            NeighborhoodCode = building.NeighborhoodCode,
            BuildingNumber = building.BuildingNumber,

            // Location Names
            GovernorateName = building.GovernorateName,
            DistrictName = building.DistrictName,
            SubDistrictName = building.SubDistrictName,
            CommunityName = building.CommunityName,
            NeighborhoodName = building.NeighborhoodName,

            // Attributes
            BuildingType = (int)building.BuildingType,
            Status = (int)building.Status,
            DamageLevel = building.DamageLevel.HasValue ? (int?)building.DamageLevel : null,
            NumberOfPropertyUnits = building.NumberOfPropertyUnits,
            NumberOfApartments = building.NumberOfApartments,
            NumberOfShops = building.NumberOfShops,
            NumberOfFloors = building.NumberOfFloors,
            YearOfConstruction = building.YearOfConstruction,

            // Location
            Latitude = building.Latitude,
            Longitude = building.Longitude,
            BuildingGeometryWkt = building.BuildingGeometryWkt,

            // Additional Information
            Address = building.Address,
            Landmark = building.Landmark,
            LocationDescription = building.LocationDescription,
            Notes = building.Notes,

            // Audit
            CreatedAtUtc = building.CreatedAtUtc,
            LastModifiedAtUtc = building.LastModifiedAtUtc
        };
    }
}