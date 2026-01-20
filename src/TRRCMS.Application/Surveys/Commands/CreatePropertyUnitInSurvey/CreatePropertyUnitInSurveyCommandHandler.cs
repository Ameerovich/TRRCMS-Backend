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
/// </summary>
public class CreatePropertyUnitInSurveyCommandHandler : IRequestHandler<CreatePropertyUnitInSurveyCommand, PropertyUnitDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public CreatePropertyUnitInSurveyCommandHandler(
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

    public async Task<PropertyUnitDto> Handle(CreatePropertyUnitInSurveyCommand request, CancellationToken cancellationToken)
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
        var building = await _buildingRepository.GetByIdAsync(survey.BuildingId, cancellationToken);
        if (building == null)
        {
            throw new NotFoundException($"Building with ID {survey.BuildingId} not found");
        }

        // Check if unit identifier already exists in this building
        var existingUnit = await _propertyUnitRepository.GetByBuildingAndIdentifierAsync(
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
            unitType: request.UnitType,
            floorNumber: request.FloorNumber,
            positionOnFloor: request.PositionOnFloor,
            createdByUserId: currentUserId
        );

        // Update additional details
        propertyUnit.UpdateDetails(
            occupancyStatus: request.OccupancyStatus,
            numberOfRooms: request.NumberOfRooms,
            estimatedAreaSqm: request.EstimatedAreaSqm,
            description: request.Description,
            modifiedByUserId: currentUserId
        );

        // Update utilities
        propertyUnit.UpdateUtilities(
            hasElectricity: request.HasElectricity ?? false,
            hasWater: request.HasWater ?? false,
            hasSewage: request.HasSewage ?? false,
            utilitiesNotes: request.UtilitiesNotes,
            modifiedByUserId: currentUserId
        );

        // Save property unit
        await _propertyUnitRepository.AddAsync(propertyUnit, cancellationToken);
        await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

        // Link property unit to survey
        survey.LinkToPropertyUnit(propertyUnit.Id, currentUserId);
        await _surveyRepository.UpdateAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

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
                propertyUnit.UnitType,
                propertyUnit.FloorNumber,
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