using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdateOfficeSurvey;

/// <summary>
/// Handler for UpdateOfficeSurveyCommand
/// Updates existing office survey while in Draft status
/// UC-004/UC-005: Office Survey update workflow
/// </summary>
public class UpdateOfficeSurveyCommandHandler : IRequestHandler<UpdateOfficeSurveyCommand, SurveyDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateOfficeSurveyCommandHandler(
        ISurveyRepository surveyRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SurveyDto> Handle(UpdateOfficeSurveyCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get existing survey
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify this is an office survey
        if (survey.Type != SurveyType.Office)
        {
            throw new ValidationException("This endpoint is only for office surveys. Use the field survey endpoints for field surveys.");
        }

        // Verify survey can be modified (must be Draft)
        survey.EnsureCanModify();

        // Capture old values for audit
        var oldValues = new
        {
            survey.PropertyUnitId,
            survey.IntervieweeName,
            survey.IntervieweeRelationship,
            survey.Notes,
            survey.DurationMinutes,
            survey.OfficeLocation,
            survey.RegistrationNumber,
            survey.AppointmentReference,
            survey.ContactPhone,
            survey.ContactEmail,
            survey.InPersonVisit
        };

        // Validate and update property unit if changed
        if (request.PropertyUnitId.HasValue && request.PropertyUnitId != survey.PropertyUnitId)
        {
            var propertyUnit = await _propertyUnitRepository.GetByIdAsync(
                request.PropertyUnitId.Value,
                cancellationToken);

            if (propertyUnit == null)
            {
                throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
            }

            // Verify property unit belongs to the survey's building
            if (propertyUnit.BuildingId != survey.BuildingId)
            {
                throw new ValidationException(
                    $"Property unit {request.PropertyUnitId} does not belong to building {survey.BuildingId}");
            }

            survey.LinkToPropertyUnit(request.PropertyUnitId.Value, currentUserId);
        }

        // Update common survey details
        survey.UpdateSurveyDetails(
            gpsCoordinates: null, // No GPS for office surveys
            intervieweeName: request.IntervieweeName ?? survey.IntervieweeName,
            intervieweeRelationship: request.IntervieweeRelationship ?? survey.IntervieweeRelationship,
            notes: request.Notes ?? survey.Notes,
            durationMinutes: request.DurationMinutes ?? survey.DurationMinutes,
            modifiedByUserId: currentUserId
        );

        // Update office-specific details
        survey.UpdateOfficeDetails(
            officeLocation: request.OfficeLocation ?? survey.OfficeLocation,
            registrationNumber: request.RegistrationNumber ?? survey.RegistrationNumber,
            appointmentReference: request.AppointmentReference ?? survey.AppointmentReference,
            contactPhone: request.ContactPhone ?? survey.ContactPhone,
            contactEmail: request.ContactEmail ?? survey.ContactEmail,
            inPersonVisit: request.InPersonVisit ?? survey.InPersonVisit,
            modifiedByUserId: currentUserId
        );

        // Save changes
        await _surveyRepository.UpdateAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

        // Build changed fields list
        var changedFields = new List<string>();
        if (request.PropertyUnitId != oldValues.PropertyUnitId) changedFields.Add("PropertyUnitId");
        if (request.IntervieweeName != oldValues.IntervieweeName) changedFields.Add("IntervieweeName");
        if (request.IntervieweeRelationship != oldValues.IntervieweeRelationship) changedFields.Add("IntervieweeRelationship");
        if (request.Notes != oldValues.Notes) changedFields.Add("Notes");
        if (request.DurationMinutes != oldValues.DurationMinutes) changedFields.Add("DurationMinutes");
        if (request.OfficeLocation != oldValues.OfficeLocation) changedFields.Add("OfficeLocation");
        if (request.RegistrationNumber != oldValues.RegistrationNumber) changedFields.Add("RegistrationNumber");
        if (request.AppointmentReference != oldValues.AppointmentReference) changedFields.Add("AppointmentReference");
        if (request.ContactPhone != oldValues.ContactPhone) changedFields.Add("ContactPhone");
        if (request.ContactEmail != oldValues.ContactEmail) changedFields.Add("ContactEmail");
        if (request.InPersonVisit != oldValues.InPersonVisit) changedFields.Add("InPersonVisit");

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated office survey {survey.ReferenceCode}",
            entityType: "Survey",
            entityId: survey.Id,
            entityIdentifier: survey.ReferenceCode,
            oldValues: System.Text.Json.JsonSerializer.Serialize(oldValues),
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                survey.PropertyUnitId,
                survey.IntervieweeName,
                survey.IntervieweeRelationship,
                survey.Notes,
                survey.DurationMinutes,
                survey.OfficeLocation,
                survey.RegistrationNumber,
                survey.AppointmentReference,
                survey.ContactPhone,
                survey.ContactEmail,
                survey.InPersonVisit
            }),
            changedFields: string.Join(", ", changedFields),
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<SurveyDto>(survey);
        result.FieldCollectorName = _currentUserService.Username;

        return result;
    }
}
