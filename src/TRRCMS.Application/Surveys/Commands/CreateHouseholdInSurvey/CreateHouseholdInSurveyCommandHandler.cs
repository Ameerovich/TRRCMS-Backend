using AutoMapper;
using MediatR;
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

    public CreateHouseholdInSurveyCommandHandler(
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

    public async Task<HouseholdDto> Handle(CreateHouseholdInSurveyCommand request, CancellationToken cancellationToken)
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

        // Verify ownership
        if (survey.FieldCollectorId != currentUserId)
        {
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
        if (propertyUnit == null)
        {
            throw new NotFoundException($"Property unit with ID {propertyUnitId} not found");
        }

        // Create household with full composition
        // Note: HeadOfHouseholdName is not set here - use SetHouseholdHead endpoint to link a Person as head
        var household = Household.Create(
            propertyUnitId: propertyUnitId.Value,
            headOfHouseholdName: null,
            householdSize: request.HouseholdSize,
            maleCount: request.MaleCount,
            femaleCount: request.FemaleCount,
            maleChildCount: request.MaleChildCount,
            femaleChildCount: request.FemaleChildCount,
            maleElderlyCount: request.MaleElderlyCount,
            femaleElderlyCount: request.FemaleElderlyCount,
            maleDisabledCount: request.MaleDisabledCount,
            femaleDisabledCount: request.FemaleDisabledCount,
            notes: request.Notes,
            occupancyType: request.OccupancyType.HasValue ? (OccupancyType)request.OccupancyType.Value : (OccupancyType?)null,
            occupancyNature: request.OccupancyNature.HasValue ? (OccupancyNature)request.OccupancyNature.Value : (OccupancyNature?)null,
            createdByUserId: currentUserId
        );

        // Save household
        await _unitOfWork.Households.AddAsync(household, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        // Map to DTO
        var result = _mapper.Map<HouseholdDto>(household);
        result.PropertyUnitIdentifier = propertyUnit.UnitIdentifier;

        return result;
    }
}
