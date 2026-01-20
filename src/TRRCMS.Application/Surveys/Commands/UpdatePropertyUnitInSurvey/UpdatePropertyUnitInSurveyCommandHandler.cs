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
            propertyUnit.FloorNumber,
            propertyUnit.PositionOnFloor,
            propertyUnit.OccupancyStatus,
            propertyUnit.NumberOfRooms,
            propertyUnit.EstimatedAreaSqm,
            propertyUnit.Description,
            propertyUnit.HasElectricity,
            propertyUnit.HasWater,
            propertyUnit.HasSewage,
            propertyUnit.UtilitiesNotes
        });

        // Update details if provided
        if (request.FloorNumber.HasValue || request.PositionOnFloor != null)
        {
            propertyUnit.UpdateLocation(
                floorNumber: request.FloorNumber,
                positionOnFloor: request.PositionOnFloor,
                modifiedByUserId: currentUserId
            );
        }

        // Update details
        propertyUnit.UpdateDetails(
            occupancyStatus: request.OccupancyStatus,
            numberOfRooms: request.NumberOfRooms,
            estimatedAreaSqm: request.EstimatedAreaSqm,
            description: request.Description,
            modifiedByUserId: currentUserId
        );

        // Update utilities if any provided
        if (request.HasElectricity.HasValue || request.HasWater.HasValue ||
            request.HasSewage.HasValue || !string.IsNullOrWhiteSpace(request.UtilitiesNotes))
        {
            propertyUnit.UpdateUtilities(
                hasElectricity: request.HasElectricity ?? propertyUnit.HasElectricity,
                hasWater: request.HasWater ?? propertyUnit.HasWater,
                hasSewage: request.HasSewage ?? propertyUnit.HasSewage,
                utilitiesNotes: request.UtilitiesNotes,
                modifiedByUserId: currentUserId
            );
        }

        // Save changes
        await _propertyUnitRepository.UpdateAsync(propertyUnit, cancellationToken);
        await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

        // Track changes
        var newValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            propertyUnit.FloorNumber,
            propertyUnit.PositionOnFloor,
            propertyUnit.OccupancyStatus,
            propertyUnit.NumberOfRooms,
            propertyUnit.EstimatedAreaSqm,
            propertyUnit.Description,
            propertyUnit.HasElectricity,
            propertyUnit.HasWater,
            propertyUnit.HasSewage,
            propertyUnit.UtilitiesNotes
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