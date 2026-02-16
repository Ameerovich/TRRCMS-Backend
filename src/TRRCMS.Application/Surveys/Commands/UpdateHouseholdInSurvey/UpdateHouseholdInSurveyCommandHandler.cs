using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdateHouseholdInSurvey;

/// <summary>
/// Handler for UpdateHouseholdInSurveyCommand
/// Updates household details in the context of a survey
/// </summary>
public class UpdateHouseholdInSurveyCommandHandler : IRequestHandler<UpdateHouseholdInSurveyCommand, HouseholdDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateHouseholdInSurveyCommandHandler(
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<HouseholdDto> Handle(UpdateHouseholdInSurveyCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("You can only update households for your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot update households for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Get household
        var household = await _householdRepository.GetByIdAsync(request.HouseholdId, cancellationToken);
        if (household == null)
        {
            throw new NotFoundException($"Household with ID {request.HouseholdId} not found");
        }

        // Update property unit if provided
        if (request.PropertyUnitId.HasValue && request.PropertyUnitId.Value != household.PropertyUnitId)
        {
            var newPropertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId.Value, cancellationToken);
            if (newPropertyUnit == null)
            {
                throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
            }

            // Verify property unit belongs to the survey's building
            if (newPropertyUnit.BuildingId != survey.BuildingId)
            {
                throw new ValidationException(
                    $"Property unit {request.PropertyUnitId} does not belong to survey building {survey.BuildingId}");
            }

            household.UpdatePropertyUnit(request.PropertyUnitId.Value, currentUserId);
        }

        // Track old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            household.PropertyUnitId,
            household.HeadOfHouseholdName,
            household.HouseholdSize,
            household.MaleCount,
            household.FemaleCount,
            household.MaleChildCount,
            household.FemaleChildCount,
            household.MaleElderlyCount,
            household.FemaleElderlyCount,
            household.MaleDisabledCount,
            household.FemaleDisabledCount,
            household.Notes
        });

        // Update basic info if provided
        // Note: HeadOfHouseholdName is managed via SetHouseholdHead endpoint, not here
        if (request.HouseholdSize.HasValue ||
            request.Notes != null ||
            request.OccupancyType.HasValue ||
            request.OccupancyNature.HasValue)
        {
            household.UpdateBasicInfo(
                headOfHouseholdName: household.HeadOfHouseholdName,
                householdSize: request.HouseholdSize ?? household.HouseholdSize,
                notes: request.Notes ?? household.Notes,
                occupancyType: request.OccupancyType.HasValue ? (OccupancyType)request.OccupancyType.Value : household.OccupancyType,
                occupancyNature: request.OccupancyNature.HasValue ? (OccupancyNature)request.OccupancyNature.Value : household.OccupancyNature,
                modifiedByUserId: currentUserId
            );
        }

        // Update composition if any field provided
        if (request.MaleCount.HasValue || request.FemaleCount.HasValue ||
            request.MaleChildCount.HasValue || request.FemaleChildCount.HasValue ||
            request.MaleElderlyCount.HasValue || request.FemaleElderlyCount.HasValue ||
            request.MaleDisabledCount.HasValue || request.FemaleDisabledCount.HasValue)
        {
            household.UpdateComposition(
                maleCount: request.MaleCount ?? household.MaleCount,
                femaleCount: request.FemaleCount ?? household.FemaleCount,
                maleChildCount: request.MaleChildCount ?? household.MaleChildCount,
                femaleChildCount: request.FemaleChildCount ?? household.FemaleChildCount,
                maleElderlyCount: request.MaleElderlyCount ?? household.MaleElderlyCount,
                femaleElderlyCount: request.FemaleElderlyCount ?? household.FemaleElderlyCount,
                maleDisabledCount: request.MaleDisabledCount ?? household.MaleDisabledCount,
                femaleDisabledCount: request.FemaleDisabledCount ?? household.FemaleDisabledCount,
                modifiedByUserId: currentUserId
            );
        }

        // Save changes
        await _householdRepository.UpdateAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        // Track new values
        var newValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            household.PropertyUnitId,
            household.HeadOfHouseholdName,
            household.HouseholdSize,
            household.MaleCount,
            household.FemaleCount,
            household.MaleChildCount,
            household.FemaleChildCount,
            household.MaleElderlyCount,
            household.FemaleElderlyCount,
            household.MaleDisabledCount,
            household.FemaleDisabledCount,
            household.Notes
        });

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated household {household.HeadOfHouseholdName} in survey {survey.ReferenceCode}",
            entityType: "Household",
            entityId: household.Id,
            entityIdentifier: household.HeadOfHouseholdName,
            oldValues: oldValues,
            newValues: newValues,
            changedFields: "Household updates",
            cancellationToken: cancellationToken
        );

        // Get property unit for DTO
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);

        // Map to DTO
        var result = _mapper.Map<HouseholdDto>(household);
        result.PropertyUnitIdentifier = propertyUnit?.UnitIdentifier;

        return result;
    }
}
