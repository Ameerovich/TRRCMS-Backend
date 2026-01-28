using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PropertyUnits.Commands.CreatePropertyUnit;

/// <summary>
/// Handler for creating a new property unit
/// Returns full PropertyUnitDto
/// </summary>
public class CreatePropertyUnitCommandHandler : IRequestHandler<CreatePropertyUnitCommand, PropertyUnitDto>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public CreatePropertyUnitCommandHandler(
        IPropertyUnitRepository propertyUnitRepository,
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _propertyUnitRepository = propertyUnitRepository;
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<PropertyUnitDto> Handle(CreatePropertyUnitCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var userId = _currentUserService.UserId ?? Guid.NewGuid();

        // Validate building exists
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId, cancellationToken);
        if (building == null)
        {
            throw new InvalidOperationException($"Building with ID {request.BuildingId} not found");
        }

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

        // Update status
        propertyUnit.UpdateStatus((PropertyUnitStatus)request.Status, null, userId);

        // Update physical details if provided
        if (request.AreaSquareMeters.HasValue || request.NumberOfRooms.HasValue)
        {
            propertyUnit.UpdatePhysicalDetails(
                numberOfRooms: request.NumberOfRooms,
                numberOfBathrooms: null,
                hasBalcony: null,
                areaSquareMeters: request.AreaSquareMeters,
                specialFeatures: null,
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

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created property unit {request.UnitIdentifier} in building {building.BuildingNumber}",
            entityType: "PropertyUnit",
            entityId: propertyUnit.Id,
            entityIdentifier: request.UnitIdentifier,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                propertyUnit.UnitIdentifier,
                UnitType = propertyUnit.UnitType.ToString(),
                Status = propertyUnit.Status.ToString(),
                propertyUnit.FloorNumber,
                propertyUnit.AreaSquareMeters,
                propertyUnit.NumberOfRooms
            }),
            changedFields: "New Property Unit",
            cancellationToken: cancellationToken
        );

        // Map to DTO and return
        var result = _mapper.Map<PropertyUnitDto>(propertyUnit);
        result.BuildingNumber = building.BuildingNumber;

        return result;
    }
}