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
/// Handler for creating a new household
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

        // Validate property unit exists
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId, cancellationToken);
        if (propertyUnit == null)
        {
            throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
        }

        // Create household with full composition
        var household = Household.Create(
            propertyUnitId: request.PropertyUnitId,
            headOfHouseholdName: request.HeadOfHouseholdName,
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
            occupancyType: null, // Not supported in CreateHousehold command (use CreateHouseholdInSurvey instead)
            occupancyNature: null, // Not supported in CreateHousehold command (use CreateHouseholdInSurvey instead)
            createdByUserId: userId
        );

        // Save to database
        await _householdRepository.AddAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created household for {request.HeadOfHouseholdName} in property unit",
            entityType: "Household",
            entityId: household.Id,
            entityIdentifier: request.HeadOfHouseholdName,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                household.HeadOfHouseholdName,
                household.HouseholdSize,
                household.MaleCount,
                household.FemaleCount
            }),
            changedFields: "New Household",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<HouseholdDto>(household);
        result.PropertyUnitIdentifier = propertyUnit.UnitIdentifier;

        return result;
    }
}
