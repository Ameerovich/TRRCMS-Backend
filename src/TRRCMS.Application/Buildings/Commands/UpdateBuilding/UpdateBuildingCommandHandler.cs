using MediatR;
using System.Text.Json;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuilding;

/// <summary>
/// Handler for UpdateBuildingCommand
/// Updates building attributes matching frontend form fields
/// </summary>
public class UpdateBuildingCommandHandler : IRequestHandler<UpdateBuildingCommand, BuildingDto>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdateBuildingCommandHandler(
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<BuildingDto> Handle(UpdateBuildingCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        // Get building
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId, cancellationToken)
            ?? throw new NotFoundException($"Building with ID {request.BuildingId} not found");

        // Track changes for audit
        var changedFields = new List<string>();
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        // Update administrative codes (building code - 17 digits) if provided
        if (!string.IsNullOrEmpty(request.GovernorateCode) &&
            !string.IsNullOrEmpty(request.DistrictCode) &&
            !string.IsNullOrEmpty(request.SubDistrictCode) &&
            !string.IsNullOrEmpty(request.CommunityCode) &&
            !string.IsNullOrEmpty(request.NeighborhoodCode) &&
            !string.IsNullOrEmpty(request.BuildingNumber) &&
            !string.IsNullOrEmpty(request.GovernorateName) &&
            !string.IsNullOrEmpty(request.DistrictName) &&
            !string.IsNullOrEmpty(request.SubDistrictName) &&
            !string.IsNullOrEmpty(request.CommunityName) &&
            !string.IsNullOrEmpty(request.NeighborhoodName))
        {
            oldValues["BuildingId"] = building.BuildingId;
            newValues["BuildingId"] = $"{request.GovernorateCode}{request.DistrictCode}{request.SubDistrictCode}" +
                                      $"{request.CommunityCode}{request.NeighborhoodCode}{request.BuildingNumber}";
            changedFields.Add("BuildingCode");

            building.UpdateAdministrativeCodes(
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
                currentUserId);
        }

        // Update building type
        if (request.BuildingType.HasValue && request.BuildingType.Value != building.BuildingType)
        {
            oldValues["BuildingType"] = building.BuildingType.ToString();
            newValues["BuildingType"] = request.BuildingType.Value.ToString();
            changedFields.Add("BuildingType");

            building.UpdateBuildingType(request.BuildingType.Value, currentUserId);
        }

        // Update building status
        if (request.BuildingStatus.HasValue && request.BuildingStatus.Value != building.Status)
        {
            oldValues["Status"] = building.Status.ToString();
            newValues["Status"] = request.BuildingStatus.Value.ToString();
            changedFields.Add("Status");

            building.UpdateStatus(request.BuildingStatus.Value, building.DamageLevel, currentUserId);
        }

        // Update unit counts
        if (request.NumberOfPropertyUnits.HasValue ||
            request.NumberOfApartments.HasValue ||
            request.NumberOfShops.HasValue)
        {
            var newPropertyUnits = request.NumberOfPropertyUnits ?? building.NumberOfPropertyUnits;
            var newApartments = request.NumberOfApartments ?? building.NumberOfApartments;
            var newShops = request.NumberOfShops ?? building.NumberOfShops;

            if (newPropertyUnits != building.NumberOfPropertyUnits ||
                newApartments != building.NumberOfApartments ||
                newShops != building.NumberOfShops)
            {
                oldValues["NumberOfPropertyUnits"] = building.NumberOfPropertyUnits;
                oldValues["NumberOfApartments"] = building.NumberOfApartments;
                oldValues["NumberOfShops"] = building.NumberOfShops;
                newValues["NumberOfPropertyUnits"] = newPropertyUnits;
                newValues["NumberOfApartments"] = newApartments;
                newValues["NumberOfShops"] = newShops;
                changedFields.Add("UnitCounts");

                building.UpdateUnitCounts(newPropertyUnits, newApartments, newShops, currentUserId);
            }
        }

        // Update coordinates
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            if (request.Latitude.Value != building.Latitude || request.Longitude.Value != building.Longitude)
            {
                oldValues["Latitude"] = building.Latitude;
                oldValues["Longitude"] = building.Longitude;
                newValues["Latitude"] = request.Latitude.Value;
                newValues["Longitude"] = request.Longitude.Value;
                changedFields.Add("Coordinates");

                building.SetCoordinates(request.Latitude.Value, request.Longitude.Value, currentUserId);
            }
        }

        // Update location description and notes
        bool descriptionChanged = false;
        var newLocationDescription = request.LocationDescription ?? building.LocationDescription;
        var newNotes = request.Notes ?? building.Notes;

        if (newLocationDescription != building.LocationDescription)
        {
            oldValues["LocationDescription"] = building.LocationDescription;
            newValues["LocationDescription"] = newLocationDescription;
            changedFields.Add("LocationDescription");
            descriptionChanged = true;
        }

        if (newNotes != building.Notes)
        {
            oldValues["Notes"] = building.Notes;
            newValues["Notes"] = newNotes;
            changedFields.Add("Notes");
            descriptionChanged = true;
        }

        if (descriptionChanged)
        {
            building.UpdateLocationInfo(newLocationDescription, newNotes, currentUserId);
        }

        // Save changes
        await _buildingRepository.UpdateAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        // Log audit entry
        if (changedFields.Any())
        {
            await _auditService.LogActionAsync(
                actionType: AuditActionType.Update,
                actionDescription: $"Updated building {building.BuildingId}",
                entityType: "Building",
                entityId: building.Id,
                entityIdentifier: building.BuildingId,
                oldValues: JsonSerializer.Serialize(oldValues),
                newValues: JsonSerializer.Serialize(newValues),
                changedFields: string.Join(", ", changedFields),
                cancellationToken: cancellationToken);
        }

        // Return updated building DTO
        return MapToDto(building);
    }

    private static BuildingDto MapToDto(Domain.Entities.Building building)
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
            BuildingType = building.BuildingType.ToString(),
            Status = building.Status.ToString(),
            DamageLevel = building.DamageLevel?.ToString(),
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