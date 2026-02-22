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
/// UC-001: Conduct Field Survey
/// </summary>
public class CreateFieldSurveyCommandHandler : IRequestHandler<CreateFieldSurveyCommand, SurveyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISurveyReferenceCodeGenerator _refCodeGenerator;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public CreateFieldSurveyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ISurveyReferenceCodeGenerator refCodeGenerator,
        IAuditService auditService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _refCodeGenerator = refCodeGenerator ?? throw new ArgumentNullException(nameof(refCodeGenerator));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SurveyDto> Handle(CreateFieldSurveyCommand request, CancellationToken cancellationToken)
    {
        // Get current user (field collector)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate building exists
        var building = await _unitOfWork.Buildings.GetByIdAsync(request.BuildingId, cancellationToken);
        if (building == null)
        {
            throw new NotFoundException($"Building with ID {request.BuildingId} not found");
        }

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

            // Verify property unit belongs to the building
            if (propertyUnit.BuildingId != request.BuildingId)
            {
                throw new ValidationException(
                    $"Property unit {request.PropertyUnitId} does not belong to building {request.BuildingId}");
            }
        }

        // Generate reference code via PostgreSQL sequence
        var referenceCode = await _refCodeGenerator.GenerateNextAsync("ALG", cancellationToken);

        // Create survey entity using new factory method
        var survey = Survey.CreateFieldSurvey(
            buildingId: request.BuildingId,
            fieldCollectorId: currentUserId,
            surveyDate: request.SurveyDate,
            propertyUnitId: request.PropertyUnitId,
            createdByUserId: currentUserId
        );

        // Set reference code using the proper domain method
        survey.SetReferenceCode(referenceCode);

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
        await _unitOfWork.Surveys.AddAsync(survey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created field survey with reference code {referenceCode} for building {building.BuildingNumber}",
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
                survey.SurveyType,
                survey.Type,
                survey.Source
            }),
            changedFields: "New Field Survey",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<SurveyDto>(survey);
        result.BuildingNumber = building.BuildingNumber;
        result.BuildingAddress = building.Address;
        result.FieldCollectorName = _currentUserService.Username;

        return result;
    }

}
