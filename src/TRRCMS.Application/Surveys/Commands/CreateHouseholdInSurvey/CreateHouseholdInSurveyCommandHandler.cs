using System.Diagnostics;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;

/// <summary>
/// Handler for CreateHouseholdInSurveyCommand
/// Creates household and links it to the survey
/// </summary>
public class CreateHouseholdInSurveyCommandHandler : IRequestHandler<CreateHouseholdInSurveyCommand, HouseholdDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateHouseholdInSurveyCommandHandler> _logger;

    public CreateHouseholdInSurveyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper,
        ILogger<CreateHouseholdInSurveyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HouseholdDto> Handle(CreateHouseholdInSurveyCommand request, CancellationToken cancellationToken)
    {
        // Per-step timing — helps diagnose hangs reported behind ngrok/proxy. Lap times
        // pinpoint which DB call or the audit write is responsible when a request stalls.
        var sw = Stopwatch.StartNew();
        long Lap(string label) { var ms = sw.ElapsedMilliseconds; _logger.LogInformation("CreateHousehold timing | {Step} | t+{Ms}ms", label, ms); return ms; }

        Lap("handler.start");

        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");
        Lap("currentUser.resolved");

        // Get and validate survey
        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken);
        Lap("survey.fetched");
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership
        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            Lap("currentUser.dbFetch");
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_EditAll))
                throw new UnauthorizedAccessException("You can only create households for your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot create households for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Determine property unit ID (from request or survey)
        var propertyUnitId = request.PropertyUnitId ?? survey.PropertyUnitId;
        if (!propertyUnitId.HasValue)
        {
            throw new ValidationException("Property unit is required. Either link property unit to survey first or provide PropertyUnitId in request.");
        }

        // Validate property unit exists
        var propertyUnit = await _unitOfWork.PropertyUnits.GetByIdAsync(propertyUnitId.Value, cancellationToken);
        Lap("propertyUnit.fetched");
        if (propertyUnit == null)
        {
            throw new NotFoundException($"Property unit with ID {propertyUnitId} not found");
        }

        // Create household with canonical v1.9 composition
        var household = Household.Create(
            propertyUnitId: propertyUnitId.Value,
            householdSize: request.HouseholdSize,
            maleCount: request.MaleCount,
            femaleCount: request.FemaleCount,
            adultCount: request.AdultCount,
            childCount: request.ChildCount,
            elderlyCount: request.ElderlyCount,
            disabledCount: request.DisabledCount,
            occupancyNature: request.OccupancyNature.HasValue ? (OccupancyNature)request.OccupancyNature.Value : (OccupancyNature?)null,
            occupancyStartDate: request.OccupancyStartDate,
            notes: request.Notes,
            createdByUserId: currentUserId
        );

        // Save household
        await _unitOfWork.Households.AddAsync(household, cancellationToken);
        Lap("household.added");
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        Lap("saveChanges.done");

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created household in survey {survey.ReferenceCode}",
            entityType: "Household",
            entityId: household.Id,
            entityIdentifier: household.Id.ToString(),
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                household.HouseholdSize,
                household.MaleCount,
                household.FemaleCount,
                request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode
            }),
            changedFields: "New Household in Survey",
            cancellationToken: cancellationToken
        );
        Lap("audit.done");

        // Map to DTO
        var result = _mapper.Map<HouseholdDto>(household);
        result.PropertyUnitIdentifier = propertyUnit.UnitIdentifier;
        Lap("dto.mapped");
        _logger.LogInformation("CreateHousehold completed in {TotalMs}ms (surveyId={SurveyId})", sw.ElapsedMilliseconds, request.SurveyId);

        return result;
    }
}
