using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PropertyUnits.Commands.CreatePropertyUnit;

/// <summary>
/// Handler for creating a new property unit
/// </summary>
public class CreatePropertyUnitCommandHandler : IRequestHandler<CreatePropertyUnitCommand, Guid>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;

    public CreatePropertyUnitCommandHandler(IPropertyUnitRepository propertyUnitRepository)
    {
        _propertyUnitRepository = propertyUnitRepository;
    }

    public async Task<Guid> Handle(CreatePropertyUnitCommand request, CancellationToken cancellationToken)
    {
        // TODO: Replace with actual authenticated user ID when authentication is implemented
        var userId = Guid.NewGuid();

        // Validate BuildingId exists (might want to add a BuildingRepository check here later)

        // Check if unit identifier already exists for this building
        var existingUnit = await _propertyUnitRepository.GetByBuildingAndIdentifierAsync(
            request.BuildingId,
            request.UnitIdentifier,
            cancellationToken);

        if (existingUnit != null)
        {
            throw new InvalidOperationException(
                $"Property unit with identifier '{request.UnitIdentifier}' already exists in this building.");
        }

        // Create property unit using factory method
        var propertyUnit = PropertyUnit.Create(
            buildingId: request.BuildingId,
            unitIdentifier: request.UnitIdentifier,
            unitType: (PropertyUnitType)request.UnitType,
            floorNumber: request.FloorNumber,
            createdByUserId: userId
        );

        // Update optional physical details if provided
        if (request.AreaSquareMeters.HasValue ||
            request.NumberOfRooms.HasValue ||
            request.NumberOfBathrooms.HasValue ||
            request.HasBalcony.HasValue ||
            !string.IsNullOrWhiteSpace(request.SpecialFeatures))
        {
            propertyUnit.UpdatePhysicalDetails(
                numberOfRooms: request.NumberOfRooms,
                numberOfBathrooms: request.NumberOfBathrooms,
                hasBalcony: request.HasBalcony,
                areaSquareMeters: request.AreaSquareMeters,
                specialFeatures: request.SpecialFeatures,
                modifiedByUserId: userId
            );
        }

        // Update occupancy information if provided
        if (request.OccupancyType.HasValue ||
            request.OccupancyNature.HasValue ||
            request.NumberOfHouseholds.HasValue ||
            request.TotalOccupants.HasValue)
        {
            propertyUnit.UpdateOccupancyInfo(
                occupancyType: request.OccupancyType.HasValue ? (OccupancyType)request.OccupancyType.Value : null,
                occupancyNature: request.OccupancyNature.HasValue ? (OccupancyNature)request.OccupancyNature.Value : null,
                numberOfHouseholds: request.NumberOfHouseholds,
                totalOccupants: request.TotalOccupants,
                modifiedByUserId: userId
            );
        }

        // Update description if provided
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            propertyUnit.UpdateDescription(request.Description, userId);
        }

        // Save to database
        await _propertyUnitRepository.AddAsync(propertyUnit, cancellationToken);
        await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

        return propertyUnit.Id;
    }
}