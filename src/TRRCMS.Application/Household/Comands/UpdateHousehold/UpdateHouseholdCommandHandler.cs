using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Households.Commands.UpdateHousehold;

/// <summary>
/// Handler for updating a household
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

        // Get household
        var household = await _householdRepository.GetByIdAsync(request.Id, cancellationToken);
        if (household == null)
        {
            throw new NotFoundException($"Household with ID {request.Id} not found");
        }

        // Track old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
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
        if (!string.IsNullOrEmpty(request.HeadOfHouseholdName) || 
            request.HouseholdSize.HasValue || 
            request.Notes != null)
        {
            household.UpdateBasicInfo(
                headOfHouseholdName: request.HeadOfHouseholdName ?? household.HeadOfHouseholdName,
                householdSize: request.HouseholdSize ?? household.HouseholdSize,
                notes: request.Notes ?? household.Notes,
                modifiedByUserId: userId
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
                modifiedByUserId: userId
            );
        }

        // Save changes
        await _householdRepository.UpdateAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        // Track new values for audit
        var newValues = System.Text.Json.JsonSerializer.Serialize(new
        {
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
            actionDescription: $"Updated household for {household.HeadOfHouseholdName}",
            entityType: "Household",
            entityId: household.Id,
            entityIdentifier: household.HeadOfHouseholdName,
            oldValues: oldValues,
            newValues: newValues,
            changedFields: "Household update",
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
