using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Enums;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Surveys.Commands.SaveDraftSurvey;

/// <summary>
/// Handler for SaveDraftSurveyCommand
/// Updates draft survey with new information
/// </summary>
public class SaveDraftSurveyCommandHandler : IRequestHandler<SaveDraftSurveyCommand, SurveyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public SaveDraftSurveyCommandHandler(
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

    public async Task<SurveyDto> Handle(SaveDraftSurveyCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get survey
        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership (only field collector who created survey can edit draft)
        if (survey.FieldCollectorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only edit your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot edit survey in {survey.Status} status. Only Draft surveys can be edited.");
        }

        // Track changes for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            survey.PropertyUnitId,
            survey.GpsCoordinates,
            survey.IntervieweeName,
            survey.IntervieweeRelationship,
            survey.Notes,
            survey.DurationMinutes
        });

        // Validate property unit if provided
        if (request.PropertyUnitId.HasValue)
        {
            var propertyUnit = await _unitOfWork.PropertyUnits.GetByIdAsync(
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
                    $"Property unit {request.PropertyUnitId} does not belong to survey building {survey.BuildingId}");
            }
        }

        // Update survey details
        survey.UpdateSurveyDetails(
            gpsCoordinates: request.GpsCoordinates,
            intervieweeName: request.IntervieweeName,
            intervieweeRelationship: request.IntervieweeRelationship,
            notes: request.Notes,
            durationMinutes: request.DurationMinutes,
            modifiedByUserId: currentUserId
        );

        // Update property unit if provided
        if (request.PropertyUnitId.HasValue)
        {
            survey.LinkToPropertyUnit(request.PropertyUnitId.Value, currentUserId);
        }

        // Save changes
        await _unitOfWork.Surveys.UpdateAsync(survey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Track changes
        var newValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            survey.PropertyUnitId,
            survey.GpsCoordinates,
            survey.IntervieweeName,
            survey.IntervieweeRelationship,
            survey.Notes,
            survey.DurationMinutes
        });

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated draft survey {survey.ReferenceCode}",
            entityType: "Survey",
            entityId: survey.Id,
            entityIdentifier: survey.ReferenceCode,
            oldValues: oldValues,
            newValues: newValues,
            changedFields: "Draft updates",
            cancellationToken: cancellationToken
        );

        // Map to DTO and return
        var result = _mapper.Map<SurveyDto>(survey);
        result.FieldCollectorName = _currentUserService.Username;

        return result;
    }
}