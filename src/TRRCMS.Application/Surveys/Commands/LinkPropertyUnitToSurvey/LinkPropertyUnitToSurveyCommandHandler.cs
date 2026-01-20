using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.LinkPropertyUnitToSurvey;

/// <summary>
/// Handler for LinkPropertyUnitToSurveyCommand
/// Links an existing property unit to a survey
/// </summary>
public class LinkPropertyUnitToSurveyCommandHandler : IRequestHandler<LinkPropertyUnitToSurveyCommand, SurveyDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public LinkPropertyUnitToSurveyCommandHandler(
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

    public async Task<SurveyDto> Handle(LinkPropertyUnitToSurveyCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("You can only link property units to your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot link property units to survey in {survey.Status} status. Only Draft surveys can be modified.");
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
                $"Property unit {propertyUnit.UnitIdentifier} does not belong to survey building. " +
                $"Property unit building ID: {propertyUnit.BuildingId}, Survey building ID: {survey.BuildingId}");
        }

        // Track old value for audit
        var oldPropertyUnitId = survey.PropertyUnitId;

        // Link property unit to survey
        survey.LinkToPropertyUnit(propertyUnit.Id, currentUserId);

        // Save changes
        await _surveyRepository.UpdateAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

        // Get building for audit
        var building = await _buildingRepository.GetByIdAsync(survey.BuildingId, cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Linked property unit {propertyUnit.UnitIdentifier} to survey {survey.ReferenceCode} for building {building?.BuildingNumber}",
            entityType: "Survey",
            entityId: survey.Id,
            entityIdentifier: survey.ReferenceCode,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { PropertyUnitId = oldPropertyUnitId }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new { PropertyUnitId = propertyUnit.Id, UnitIdentifier = propertyUnit.UnitIdentifier }),
            changedFields: "PropertyUnitId",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<SurveyDto>(survey);
        result.BuildingNumber = building?.BuildingNumber;
        result.BuildingAddress = building?.Address;
        result.UnitIdentifier = propertyUnit.UnitIdentifier;
        result.FieldCollectorName = _currentUserService.Username;

        return result;
    }
}