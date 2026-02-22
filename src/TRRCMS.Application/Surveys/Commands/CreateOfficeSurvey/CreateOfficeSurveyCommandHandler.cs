using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.CreateOfficeSurvey;

/// <summary>
/// Handler for CreateOfficeSurveyCommand
/// Creates new office survey with Draft status and generates reference code
/// UC-004: Office Survey
/// </summary>
public class CreateOfficeSurveyCommandHandler : IRequestHandler<CreateOfficeSurveyCommand, SurveyDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISurveyReferenceCodeGenerator _refCodeGenerator;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public CreateOfficeSurveyCommandHandler(
        ISurveyRepository surveyRepository,
        IBuildingRepository buildingRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ICurrentUserService currentUserService,
        ISurveyReferenceCodeGenerator refCodeGenerator,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _buildingRepository = buildingRepository ?? throw new ArgumentNullException(nameof(buildingRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _refCodeGenerator = refCodeGenerator ?? throw new ArgumentNullException(nameof(refCodeGenerator));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SurveyDto> Handle(CreateOfficeSurveyCommand request, CancellationToken cancellationToken)
    {
        // Get current user (office clerk)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate building exists
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId, cancellationToken);
        if (building == null)
        {
            throw new NotFoundException($"Building with ID {request.BuildingId} not found");
        }

        // Validate property unit if provided
        PropertyUnit? propertyUnit = null;
        if (request.PropertyUnitId.HasValue)
        {
            propertyUnit = await _propertyUnitRepository.GetByIdAsync(
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

        // Generate reference code via PostgreSQL sequence
        var referenceCode = await _refCodeGenerator.GenerateNextAsync("OFC", cancellationToken);

        // Create office survey entity using new factory method
        var survey = Survey.CreateOfficeSurvey(
            buildingId: request.BuildingId,
            officeClerkId: currentUserId,
            surveyDate: request.SurveyDate,
            propertyUnitId: request.PropertyUnitId,
            officeLocation: request.OfficeLocation,
            registrationNumber: request.RegistrationNumber,
            inPersonVisit: request.InPersonVisit,
            createdByUserId: currentUserId
        );

        // Set the generated reference code
        survey.SetReferenceCode(referenceCode);

        // Update survey details if provided
        survey.UpdateSurveyDetails(
            gpsCoordinates: null, // No GPS for office surveys
            intervieweeName: request.IntervieweeName,
            intervieweeRelationship: request.IntervieweeRelationship,
            notes: request.Notes,
            durationMinutes: null,
            modifiedByUserId: currentUserId
        );

        // Update office-specific details
        survey.UpdateOfficeDetails(
            officeLocation: request.OfficeLocation,
            registrationNumber: request.RegistrationNumber,
            appointmentReference: request.AppointmentReference,
            contactPhone: request.ContactPhone,
            contactEmail: request.ContactEmail,
            inPersonVisit: request.InPersonVisit,
            modifiedByUserId: currentUserId
        );

        // Save to repository
        await _surveyRepository.AddAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created office survey with reference code {referenceCode} for building {building.BuildingId}",
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
                SurveyType = "Office",
                survey.OfficeLocation,
                survey.RegistrationNumber,
                survey.InPersonVisit
            }),
            changedFields: "New Office Survey",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<SurveyDto>(survey);
        result.BuildingNumber = building.BuildingNumber;
        result.BuildingAddress = building.Address;
        result.FieldCollectorName = _currentUserService.Username; // "Clerk" for office surveys

        if (propertyUnit != null)
        {
            result.UnitIdentifier = propertyUnit.UnitIdentifier;
        }

        return result;
    }

}
