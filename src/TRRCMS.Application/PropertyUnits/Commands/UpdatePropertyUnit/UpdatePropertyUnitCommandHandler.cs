using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PropertyUnits.Commands.UpdatePropertyUnit;

/// <summary>
/// Handler for updating a property unit
/// </summary>
public class UpdatePropertyUnitCommandHandler : IRequestHandler<UpdatePropertyUnitCommand, PropertyUnitDto>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdatePropertyUnitCommandHandler(
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

    public async Task<PropertyUnitDto> Handle(UpdatePropertyUnitCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var userId = _currentUserService.UserId ?? Guid.NewGuid();

        // Get property unit
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.Id, cancellationToken);
        if (propertyUnit == null)
        {
            throw new NotFoundException($"Property unit with ID {request.Id} not found");
        }

        // Track old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            UnitType = propertyUnit.UnitType.ToString(),
            Status = propertyUnit.Status.ToString(),
            propertyUnit.FloorNumber,
            propertyUnit.AreaSquareMeters,
            propertyUnit.NumberOfRooms,
            propertyUnit.Description
        });

        // Update floor number if provided
        if (request.FloorNumber.HasValue)
        {
            propertyUnit.UpdateLocation(request.FloorNumber, null, userId);
        }

        // Update status if provided
        if (request.Status.HasValue)
        {
            propertyUnit.UpdateStatus((PropertyUnitStatus)request.Status.Value, null, userId);
        }

        // Update physical details if provided
        if (request.AreaSquareMeters.HasValue || request.NumberOfRooms.HasValue)
        {
            propertyUnit.UpdatePhysicalDetails(
                numberOfRooms: request.NumberOfRooms ?? propertyUnit.NumberOfRooms,
                numberOfBathrooms: null,
                hasBalcony: null,
                areaSquareMeters: request.AreaSquareMeters ?? propertyUnit.AreaSquareMeters,
                specialFeatures: null,
                modifiedByUserId: userId
            );
        }

        // Update description if provided
        if (request.Description != null)
        {
            propertyUnit.UpdateDescription(request.Description, userId);
        }

        // Note: UnitType is typically not updated after creation
        // If needed, add a method to the entity for this

        // Save changes
        await _propertyUnitRepository.UpdateAsync(propertyUnit, cancellationToken);
        await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

        // Track new values for audit
        var newValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            UnitType = propertyUnit.UnitType.ToString(),
            Status = propertyUnit.Status.ToString(),
            propertyUnit.FloorNumber,
            propertyUnit.AreaSquareMeters,
            propertyUnit.NumberOfRooms,
            propertyUnit.Description
        });

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated property unit {propertyUnit.UnitIdentifier}",
            entityType: "PropertyUnit",
            entityId: propertyUnit.Id,
            entityIdentifier: propertyUnit.UnitIdentifier,
            oldValues: oldValues,
            newValues: newValues,
            changedFields: "Property unit update",
            cancellationToken: cancellationToken
        );

        // Get building for DTO
        var building = await _buildingRepository.GetByIdAsync(propertyUnit.BuildingId, cancellationToken);

        // Map to DTO and return
        var result = _mapper.Map<PropertyUnitDto>(propertyUnit);
        result.BuildingNumber = building?.BuildingNumber;

        return result;
    }
}
