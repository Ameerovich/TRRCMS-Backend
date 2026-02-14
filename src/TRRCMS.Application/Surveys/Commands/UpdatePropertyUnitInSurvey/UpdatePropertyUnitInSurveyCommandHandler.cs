using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdatePropertyUnitInSurvey;

/// <summary>
/// Handler for UpdatePropertyUnitInSurveyCommand
/// Updates property unit details in the context of a survey
/// Simplified to match frontend form fields
/// </summary>
public class UpdatePropertyUnitInSurveyCommandHandler : IRequestHandler<UpdatePropertyUnitInSurveyCommand, PropertyUnitDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdatePropertyUnitInSurveyCommandHandler(
        ISurveyRepository surveyRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _buildingRepository = buildingRepository ?? throw new ArgumentNullException(nameof(buildingRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PropertyUnitDto> Handle(UpdatePropertyUnitInSurveyCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get and validate survey
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership
        if (survey.FieldCollectorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only update property units for your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot update property units for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Get property unit
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId, cancellationToken);
        if (propertyUnit == null)
        {
            throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
        }

        // Verify property unit belongs to survey's building
        if (propertyUnit.BuildingId != survey.BuildingId)
        {
            throw new ValidationException(
                $"Property unit {request.PropertyUnitId} does not belong to survey building {survey.BuildingId}");
        }

        // Track changes for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            propertyUnit.UnitIdentifier,
            UnitType = propertyUnit.UnitType.ToString(),
            Status = propertyUnit.Status.ToString(),
            propertyUnit.FloorNumber,
            propertyUnit.AreaSquareMeters,
            propertyUnit.NumberOfRooms,
            propertyUnit.Description
        });

        // Update unit identifier if provided
        if (!string.IsNullOrWhiteSpace(request.UnitIdentifier) && request.UnitIdentifier != propertyUnit.UnitIdentifier)
        {
            // Check for duplicate unit identifier within the same building
            var existingUnit = await _propertyUnitRepository.GetByBuildingAndIdentifierAsync(
                propertyUnit.BuildingId, request.UnitIdentifier, cancellationToken);
            if (existingUnit != null && existingUnit.Id != propertyUnit.Id)
            {
                throw new ValidationException(
                    $"A property unit with identifier '{request.UnitIdentifier}' already exists in this building");
            }
            propertyUnit.UpdateUnitIdentifier(request.UnitIdentifier, currentUserId);
        }

        // Update floor number if provided
        if (request.FloorNumber.HasValue)
        {
            propertyUnit.UpdateLocation(request.FloorNumber, null, currentUserId);
        }

        // Update status if provided
        if (request.Status.HasValue)
        {
            propertyUnit.UpdateStatus((PropertyUnitStatus)request.Status.Value, null, currentUserId);
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
                modifiedByUserId: currentUserId
            );
        }

        // Update description if provided
        if (request.Description != null)
        {
            propertyUnit.UpdateDescription(request.Description, currentUserId);
        }

        // Save changes
        await _propertyUnitRepository.UpdateAsync(propertyUnit, cancellationToken);
        await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

        // Track changes
        var newValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            propertyUnit.UnitIdentifier,
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
            actionDescription: $"Updated property unit {propertyUnit.UnitIdentifier} in survey {survey.ReferenceCode}",
            entityType: "PropertyUnit",
            entityId: propertyUnit.Id,
            entityIdentifier: propertyUnit.UnitIdentifier,
            oldValues: oldValues,
            newValues: newValues,
            changedFields: "Property unit updates",
            cancellationToken: cancellationToken
        );

        // Get building for DTO
        var building = await _buildingRepository.GetByIdAsync(propertyUnit.BuildingId, cancellationToken);

        // Map to DTO
        var result = _mapper.Map<PropertyUnitDto>(propertyUnit);
        result.BuildingNumber = building?.BuildingNumber;

        return result;
    }
}
