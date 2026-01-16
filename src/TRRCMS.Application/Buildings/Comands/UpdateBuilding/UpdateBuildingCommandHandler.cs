using AutoMapper;
using MediatR;
using System.Text.Json;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuilding;

public class UpdateBuildingCommandHandler : IRequestHandler<UpdateBuildingCommand, BuildingDto>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateBuildingCommandHandler(
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<BuildingDto> Handle(UpdateBuildingCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        // Get building
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId, cancellationToken)
            ?? throw new KeyNotFoundException($"Building with ID {request.BuildingId} not found");

        // Track changes for audit
        var changedFields = new List<string>();
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        // Update status and damage level
        if (request.Status.HasValue || request.DamageLevel.HasValue)
        {
            var newStatus = request.Status ?? building.Status;
            var newDamageLevel = request.DamageLevel ?? building.DamageLevel;

            if (newStatus != building.Status || newDamageLevel != building.DamageLevel)
            {
                oldValues["Status"] = building.Status.ToString();
                oldValues["DamageLevel"] = building.DamageLevel?.ToString();
                newValues["Status"] = newStatus.ToString();
                newValues["DamageLevel"] = newDamageLevel?.ToString();
                changedFields.Add("Status");
                if (request.DamageLevel.HasValue)
                    changedFields.Add("DamageLevel");

                building.UpdateStatus(newStatus, newDamageLevel, currentUserId);
            }
        }

        // Update building type
        if (request.BuildingType.HasValue && request.BuildingType.Value != building.BuildingType)
        {
            oldValues["BuildingType"] = building.BuildingType.ToString();
            newValues["BuildingType"] = request.BuildingType.Value.ToString();
            changedFields.Add("BuildingType");

            building.UpdateBuildingType(request.BuildingType.Value, currentUserId);
        }

        // Update unit counts
        if (request.NumberOfApartments.HasValue || request.NumberOfShops.HasValue)
        {
            var newApartments = request.NumberOfApartments ?? building.NumberOfApartments;
            var newShops = request.NumberOfShops ?? building.NumberOfShops;

            if (newApartments != building.NumberOfApartments || newShops != building.NumberOfShops)
            {
                oldValues["NumberOfApartments"] = building.NumberOfApartments;
                oldValues["NumberOfShops"] = building.NumberOfShops;
                newValues["NumberOfApartments"] = newApartments;
                newValues["NumberOfShops"] = newShops;
                changedFields.Add("NumberOfApartments");
                changedFields.Add("NumberOfShops");

                building.UpdateUnitCounts(newApartments, newShops, currentUserId);
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

        // Update building details (address, landmark, notes, floors, year)
        bool detailsChanged = false;
        var newFloors = request.NumberOfFloors ?? building.NumberOfFloors;
        var newYear = request.YearOfConstruction ?? building.YearOfConstruction;
        var newAddress = request.Address ?? building.Address;
        var newLandmark = request.Landmark ?? building.Landmark;
        var newNotes = request.Notes ?? building.Notes;

        if (newFloors != building.NumberOfFloors)
        {
            oldValues["NumberOfFloors"] = building.NumberOfFloors;
            newValues["NumberOfFloors"] = newFloors;
            changedFields.Add("NumberOfFloors");
            detailsChanged = true;
        }

        if (newYear != building.YearOfConstruction)
        {
            oldValues["YearOfConstruction"] = building.YearOfConstruction;
            newValues["YearOfConstruction"] = newYear;
            changedFields.Add("YearOfConstruction");
            detailsChanged = true;
        }

        if (newAddress != building.Address)
        {
            oldValues["Address"] = building.Address;
            newValues["Address"] = newAddress;
            changedFields.Add("Address");
            detailsChanged = true;
        }

        if (newLandmark != building.Landmark)
        {
            oldValues["Landmark"] = building.Landmark;
            newValues["Landmark"] = newLandmark;
            changedFields.Add("Landmark");
            detailsChanged = true;
        }

        if (newNotes != building.Notes)
        {
            oldValues["Notes"] = building.Notes;
            newValues["Notes"] = newNotes;
            changedFields.Add("Notes");
            detailsChanged = true;
        }

        if (detailsChanged)
        {
            building.UpdateDetails(
                newFloors,
                newYear,
                newAddress,
                newLandmark,
                newNotes,
                currentUserId);
        }

        // Save changes
        await _buildingRepository.UpdateAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        // Log audit entry
        if (changedFields.Any())
        {
            await _auditService.LogActionAsync(
                actionType: AuditActionType.Update,
                actionDescription: $"Updated building {building.BuildingId}. Reason: {request.ReasonForModification}",
                entityType: "Building",
                entityId: building.Id,
                entityIdentifier: building.BuildingId,
                oldValues: JsonSerializer.Serialize(oldValues),
                newValues: JsonSerializer.Serialize(newValues),
                changedFields: string.Join(", ", changedFields),
                cancellationToken: cancellationToken);
        }

        // Return updated building DTO
        return _mapper.Map<BuildingDto>(building);
    }
}