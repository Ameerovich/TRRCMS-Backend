using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Households.Commands.UpdateHousehold;

/// <summary>
/// Handler for updating a household (canonical v1.9 shape).
/// </summary>
public class UpdateHouseholdCommandHandler : IRequestHandler<UpdateHouseholdCommand, HouseholdDto>
{
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateHouseholdCommandHandler(
        IHouseholdRepository householdRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _householdRepository = householdRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<HouseholdDto> Handle(UpdateHouseholdCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? Guid.NewGuid();

        var household = await _householdRepository.GetByIdAsync(request.Id, cancellationToken);
        if (household == null)
        {
            throw new NotFoundException($"Household with ID {request.Id} not found");
        }

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
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
                modifiedByUserId: userId
            );
        }

        // Update composition if any count field provided
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
                modifiedByUserId: userId
            );
        }

        await _householdRepository.UpdateAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        var newValues = System.Text.Json.JsonSerializer.Serialize(new
        {
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
            actionDescription: $"Updated {householdIdentifier}",
            entityType: "Household",
            entityId: household.Id,
            entityIdentifier: householdIdentifier,
            oldValues: oldValues,
            newValues: newValues,
            changedFields: "Household update",
            cancellationToken: cancellationToken
        );

        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);

        var result = _mapper.Map<HouseholdDto>(household);
        result.PropertyUnitIdentifier = propertyUnit?.UnitIdentifier;

        return result;
    }
}
