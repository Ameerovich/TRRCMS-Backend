using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Households.Commands.CreateHousehold;

/// <summary>
/// Handler for creating a new household (canonical v1.9 shape).
/// </summary>
public class CreateHouseholdCommandHandler : IRequestHandler<CreateHouseholdCommand, HouseholdDto>
{
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public CreateHouseholdCommandHandler(
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

    public async Task<HouseholdDto> Handle(CreateHouseholdCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? Guid.NewGuid();

        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId, cancellationToken);
        if (propertyUnit == null)
        {
            throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
        }

        var household = Household.Create(
            propertyUnitId: request.PropertyUnitId,
            householdSize: request.HouseholdSize,
            maleCount: request.MaleCount,
            femaleCount: request.FemaleCount,
            adultCount: request.AdultCount,
            childCount: request.ChildCount,
            elderlyCount: request.ElderlyCount,
            disabledCount: request.DisabledCount,
            occupancyNature: request.OccupancyNature.HasValue ? (OccupancyNature)request.OccupancyNature.Value : null,
            occupancyStartDate: request.OccupancyStartDate,
            notes: request.Notes,
            createdByUserId: userId
        );

        await _householdRepository.AddAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        var householdIdentifier = $"Household {household.Id.ToString()[..8]}";
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created {householdIdentifier} in property unit",
            entityType: "Household",
            entityId: household.Id,
            entityIdentifier: householdIdentifier,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                household.HouseholdSize,
                household.MaleCount,
                household.FemaleCount,
                household.AdultCount,
                household.ChildCount,
                household.ElderlyCount,
                household.DisabledCount,
                household.OccupancyNature,
                household.OccupancyStartDate
            }),
            changedFields: "New Household",
            cancellationToken: cancellationToken
        );

        var result = _mapper.Map<HouseholdDto>(household);
        result.PropertyUnitIdentifier = propertyUnit.UnitIdentifier;

        return result;
    }
}
