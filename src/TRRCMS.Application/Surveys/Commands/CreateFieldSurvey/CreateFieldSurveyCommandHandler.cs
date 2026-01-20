using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.CreateFieldSurvey;

/// <summary>
/// Handler for CreateFieldSurveyCommand
/// Creates new field survey with Draft status and generates reference code
/// </summary>
public class CreateFieldSurveyCommandHandler : IRequestHandler<CreateFieldSurveyCommand, SurveyDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public CreateFieldSurveyCommandHandler(
        ISurveyRepository surveyRepository,
        IBuildingRepository buildingRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _buildingRepository = buildingRepository ?? throw new ArgumentNullException(nameof(buildingRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SurveyDto> Handle(CreateFieldSurveyCommand request, CancellationToken cancellationToken)
    {
        // Get current user (field collector)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate building exists
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId, cancellationToken);
        if (building == null)
        {
            throw new NotFoundException($"Building with ID {request.BuildingId} not found");
        }

        // Validate property unit if provided
        if (request.PropertyUnitId.HasValue)
        {
            var propertyUnit = await _propertyUnitRepository.GetByIdAsync(
                request.PropertyUnitId.Value,
                cancellationToken);

            if (propertyUnit == null)
            {
                throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
            }

            // Verify property unit belongs to the building
            if (propertyUnit.BuildingId != request.BuildingId)
            {
                throw new ValidationException(
                    $"Property unit {request.PropertyUnitId} does not belong to building {request.BuildingId}");
            }
        }

        // Generate reference code
        var referenceCode = await GenerateReferenceCodeAsync(cancellationToken);

        // Create survey entity
        var survey = Survey.Create(
            buildingId: request.BuildingId,
            fieldCollectorId: currentUserId,
            surveyType: "Field",
            surveyDate: request.SurveyDate,
            propertyUnitId: request.PropertyUnitId,
            createdByUserId: currentUserId
        );

        // Update with reference code (via reflection since property is private)
        // This is a workaround - ideally Survey.Create should accept referenceCode
        var referenceCodeProperty = typeof(Survey).GetProperty("ReferenceCode");
        referenceCodeProperty?.SetValue(survey, referenceCode);

        // Update survey details if provided
        survey.UpdateSurveyDetails(
            gpsCoordinates: request.GpsCoordinates,
            intervieweeName: request.IntervieweeName,
            intervieweeRelationship: request.IntervieweeRelationship,
            notes: request.Notes,
            durationMinutes: null,
            modifiedByUserId: currentUserId
        );

        // Save to repository
        await _surveyRepository.AddAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created field survey with reference code {referenceCode} for building {building.BuildingId}",
            entityType: "Survey",
            entityId: survey.Id,
            entityIdentifier: referenceCode,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                survey.ReferenceCode,
                survey.BuildingId,
                BuildingNumber = building.BuildingNumber,
                survey.SurveyDate,
                survey.Status,
                survey.SurveyType
            }),
            changedFields: "New Survey",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<SurveyDto>(survey);
        result.BuildingNumber = building.BuildingNumber;
        result.BuildingAddress = building.Address;
        result.FieldCollectorName = _currentUserService.Username;

        return result;
    }

    /// <summary>
    /// Generate unique reference code in format: ALG-YYYY-NNNNN
    /// </summary>
    private async Task<string> GenerateReferenceCodeAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var sequence = await _surveyRepository.GetNextReferenceSequenceAsync(cancellationToken);
        return $"ALG-{year}-{sequence:D5}";
    }
}