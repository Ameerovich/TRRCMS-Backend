using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.CreatePropertyUnitInSurvey;

/// <summary>
/// Handler for CreatePropertyUnitInSurveyCommand
/// Creates property unit and automatically links it to the survey
/// Simplified to match frontend form fields
/// </summary>
public class CreatePropertyUnitInSurveyCommandHandler : IRequestHandler<CreatePropertyUnitInSurveyCommand, PropertyUnitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public CreatePropertyUnitInSurveyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PropertyUnitDto> Handle(CreatePropertyUnitInSurveyCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get and validate survey
        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership (only field collector who created survey can create units)
        if (survey.FieldCollectorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only create property units for your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot create property units for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Get building
        var building = await _unitOfWork.Buildings.GetByIdAsync(survey.BuildingId, cancellationToken);
        if (building == null)
        {
            throw new NotFoundException($"Building with ID {survey.BuildingId} not found");
        }

        // Check if unit identifier already exists in this building
        var existingUnit = await _unitOfWork.PropertyUnits.GetByBuildingAndIdentifierAsync(
            survey.BuildingId,
            request.UnitIdentifier,
            cancellationToken);

        if (existingUnit != null)
        {
            throw new ValidationException(
                $"Property unit with identifier '{request.UnitIdentifier}' already exists in building {building.BuildingNumber}");
        }

        // Create property unit entity
        var propertyUnit = PropertyUnit.Create(
            buildingId: survey.BuildingId,
            unitIdentifier: request.UnitIdentifier,
            unitType: (PropertyUnitType)request.UnitType,
            floorNumber: request.FloorNumber,
            createdByUserId: currentUserId
        );

        // Update status
        propertyUnit.UpdateStatus((PropertyUnitStatus)request.Status, null, currentUserId);

        // Update physical details if provided
        if (request.AreaSquareMeters.HasValue || request.NumberOfRooms.HasValue)
        {
            propertyUnit.UpdatePhysicalDetails(
                numberOfRooms: request.NumberOfRooms,
                numberOfBathrooms: null,
                hasBalcony: null,
                areaSquareMeters: request.AreaSquareMeters,
                specialFeatures: null,
                modifiedByUserId: currentUserId
            );
        }

        // Update description if provided
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            propertyUnit.UpdateDescription(request.Description, currentUserId);
        }

        // Save property unit
        await _unitOfWork.PropertyUnits.AddAsync(propertyUnit, cancellationToken);

        // Link property unit to survey
        survey.LinkToPropertyUnit(propertyUnit.Id, currentUserId);
        await _unitOfWork.Surveys.UpdateAsync(survey, cancellationToken);

        // Save all changes atomically
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created property unit {request.UnitIdentifier} in survey {survey.ReferenceCode} for building {building.BuildingNumber}",
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
                propertyUnit.NumberOfRooms,
                request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode
            }),
            changedFields: "New Property Unit in Survey",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<PropertyUnitDto>(propertyUnit);
        result.BuildingNumber = building.BuildingNumber;

        return result;
    }
}
