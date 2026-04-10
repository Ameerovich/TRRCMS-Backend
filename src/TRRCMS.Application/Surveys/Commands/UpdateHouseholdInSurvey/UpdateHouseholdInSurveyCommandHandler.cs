using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdateHouseholdInSurvey;

/// <summary>
/// Handler for UpdateHouseholdInSurveyCommand (canonical v1.9 shape).
/// </summary>
public class UpdateHouseholdInSurveyCommandHandler : IRequestHandler<UpdateHouseholdInSurveyCommand, HouseholdDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateHouseholdInSurveyCommandHandler(
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

    public async Task<HouseholdDto> Handle(UpdateHouseholdInSurveyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_EditAll))
                throw new UnauthorizedAccessException("You can only update households for your own surveys");
        }

        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot update households for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        var household = await _unitOfWork.Households.GetByIdAsync(request.HouseholdId, cancellationToken);
        if (household == null)
        {
            throw new NotFoundException($"Household with ID {request.HouseholdId} not found");
        }

        if (request.PropertyUnitId.HasValue && request.PropertyUnitId.Value != household.PropertyUnitId)
        {
            var newPropertyUnit = await _unitOfWork.PropertyUnits.GetByIdAsync(request.PropertyUnitId.Value, cancellationToken);
            if (newPropertyUnit == null)
            {
                throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
            }

            if (newPropertyUnit.BuildingId != survey.BuildingId)
            {
                throw new ValidationException(
                    $"Property unit {request.PropertyUnitId} does not belong to survey building {survey.BuildingId}");
            }

            household.UpdatePropertyUnit(request.PropertyUnitId.Value, currentUserId);
        }

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            household.PropertyUnitId,
            household.HouseholdSize,
            household.MaleCount,
            household.FemaleCount,
            household.AdultCount,
            household.ChildCount,
            household.ElderlyCount,
            household.DisabledCount,
            household.OccupancyNature,
            household.OccupancyStartDate,
            household.Notes
        });

        // Update basic info if any basic field provided
        if (request.HouseholdSize.HasValue ||
            request.Notes != null ||
            request.OccupancyNature.HasValue ||
            request.OccupancyStartDate.HasValue)
        {
            household.UpdateBasicInfo(
                householdSize: request.HouseholdSize ?? household.HouseholdSize,
                notes: request.Notes ?? household.Notes,
                occupancyNature: request.OccupancyNature.HasValue
                    ? (OccupancyNature)request.OccupancyNature.Value
                    : household.OccupancyNature,
                occupancyStartDate: request.OccupancyStartDate ?? household.OccupancyStartDate,
                modifiedByUserId: currentUserId
            );
        }

        // Update composition if any count provided
        if (request.MaleCount.HasValue ||
            request.FemaleCount.HasValue ||
            request.AdultCount.HasValue ||
            request.ChildCount.HasValue ||
            request.ElderlyCount.HasValue ||
            request.DisabledCount.HasValue)
        {
            household.UpdateComposition(
                maleCount: request.MaleCount ?? household.MaleCount,
                femaleCount: request.FemaleCount ?? household.FemaleCount,
                adultCount: request.AdultCount ?? household.AdultCount,
                childCount: request.ChildCount ?? household.ChildCount,
                elderlyCount: request.ElderlyCount ?? household.ElderlyCount,
                disabledCount: request.DisabledCount ?? household.DisabledCount,
                modifiedByUserId: currentUserId
            );
        }

        await _unitOfWork.Households.UpdateAsync(household, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var newValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            household.PropertyUnitId,
            household.HouseholdSize,
            household.MaleCount,
            household.FemaleCount,
            household.AdultCount,
            household.ChildCount,
            household.ElderlyCount,
            household.DisabledCount,
            household.OccupancyNature,
            household.OccupancyStartDate,
            household.Notes
        });

        var householdIdentifier = $"Household {household.Id.ToString()[..8]}";
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated {householdIdentifier} in survey {survey.ReferenceCode}",
            entityType: "Household",
            entityId: household.Id,
            entityIdentifier: householdIdentifier,
            oldValues: oldValues,
            newValues: newValues,
            changedFields: "Household updates",
            cancellationToken: cancellationToken
        );

        var propertyUnit = await _unitOfWork.PropertyUnits.GetByIdAsync(household.PropertyUnitId, cancellationToken);

        var result = _mapper.Map<HouseholdDto>(household);
        result.PropertyUnitIdentifier = propertyUnit?.UnitIdentifier;

        return result;
    }
}
