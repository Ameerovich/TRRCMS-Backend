using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.RegisterBuilding;

/// <summary>
/// Handler for RegisterBuildingCommand (QGIS plugin).
/// Creates a building with minimal data: admin codes + polygon geometry.
/// Defaults: BuildingType=Residential, Status=Unknown, all counts=0.
/// Full details are provided later via field survey import (.uhc).
/// </summary>
public class RegisterBuildingCommandHandler : IRequestHandler<RegisterBuildingCommand, BuildingDto>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeometryConverter _geometryConverter;

    public RegisterBuildingCommandHandler(
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IGeometryConverter geometryConverter)
    {
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
        _geometryConverter = geometryConverter;
    }

    public async Task<BuildingDto> Handle(RegisterBuildingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Compose the 17-digit BuildingId
        var buildingIdCode = $"{request.GovernorateCode}{request.DistrictCode}{request.SubDistrictCode}" +
                             $"{request.CommunityCode}{request.NeighborhoodCode}{request.BuildingNumber}";

        // Check for existing building with same code
        var existing = await _buildingRepository.GetByBuildingIdAsync(buildingIdCode, cancellationToken);
        if (existing != null)
            throw new ConflictException($"Building with code '{buildingIdCode}' already exists.");

        // Create building with minimal QGIS data + defaults
        var building = Building.Create(
            governorateCode: request.GovernorateCode,
            districtCode: request.DistrictCode,
            subDistrictCode: request.SubDistrictCode,
            communityCode: request.CommunityCode,
            neighborhoodCode: request.NeighborhoodCode,
            buildingNumber: request.BuildingNumber,
            governorateName: string.Empty,
            districtName: string.Empty,
            subDistrictName: string.Empty,
            communityName: string.Empty,
            neighborhoodName: string.Empty,
            buildingType: BuildingType.Residential,
            status: BuildingStatus.Unknown,
            createdByUserId: userId);

        // Set polygon geometry from QGIS (auto-computes centroid lat/lng)
        var geometry = _geometryConverter.ParseWkt(request.BuildingGeometryWkt);
        building.SetGeometry(geometry, userId);

        // Set notes if provided
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            building.UpdateDetails(
                notes: request.Notes,
                modifiedByUserId: userId);
        }

        await _buildingRepository.AddAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(building);
    }

    private static BuildingDto MapToDto(Building building)
    {
        return new BuildingDto
        {
            Id = building.Id,
            BuildingId = building.BuildingId,

            // Administrative codes
            GovernorateCode = building.GovernorateCode,
            DistrictCode = building.DistrictCode,
            SubDistrictCode = building.SubDistrictCode,
            CommunityCode = building.CommunityCode,
            NeighborhoodCode = building.NeighborhoodCode,
            BuildingNumber = building.BuildingNumber,

            // Location names
            GovernorateName = building.GovernorateName,
            DistrictName = building.DistrictName,
            SubDistrictName = building.SubDistrictName,
            CommunityName = building.CommunityName,
            NeighborhoodName = building.NeighborhoodName,

            // Building attributes
            BuildingType = (int)building.BuildingType,
            Status = (int)building.Status,
            NumberOfPropertyUnits = building.NumberOfPropertyUnits,
            NumberOfApartments = building.NumberOfApartments,
            NumberOfShops = building.NumberOfShops,
            // Spatial
            Latitude = building.Latitude,
            Longitude = building.Longitude,
            BuildingGeometryWkt = building.BuildingGeometryWkt,

            // Additional
            Notes = building.Notes,

            // Audit
            CreatedAtUtc = building.CreatedAtUtc,
            LastModifiedAtUtc = building.LastModifiedAtUtc
        };
    }
}
