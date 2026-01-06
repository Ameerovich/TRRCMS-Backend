using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Households.Commands.CreateHousehold;

/// <summary>
/// Handler for CreateHouseholdCommand
/// </summary>
public class CreateHouseholdCommandHandler : IRequestHandler<CreateHouseholdCommand, HouseholdDto>
{
    private readonly IHouseholdRepository _householdRepository;
    private readonly IMapper _mapper;

    public CreateHouseholdCommandHandler(
        IHouseholdRepository householdRepository,
        IMapper mapper)
    {
        _householdRepository = householdRepository;
        _mapper = mapper;
    }

    public async Task<HouseholdDto> Handle(CreateHouseholdCommand request, CancellationToken cancellationToken)
    {
        // Validate required fields
        if (request.PropertyUnitId == Guid.Empty)
            throw new ArgumentException("PropertyUnitId is required", nameof(request.PropertyUnitId));

        if (string.IsNullOrWhiteSpace(request.HeadOfHouseholdName))
            throw new ArgumentException("HeadOfHouseholdName is required", nameof(request.HeadOfHouseholdName));

        if (request.HouseholdSize <= 0)
            throw new ArgumentException("HouseholdSize must be greater than 0", nameof(request.HouseholdSize));

        if (request.CreatedByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId is required", nameof(request.CreatedByUserId));

        // Create household using factory method
        var household = Household.Create(
            request.PropertyUnitId,
            request.HeadOfHouseholdName,
            request.HouseholdSize,
            request.CreatedByUserId);

        // Update optional composition fields using domain methods

        // Gender composition
        if (request.MaleCount.HasValue || request.FemaleCount.HasValue)
        {
            household.UpdateGenderComposition(
                request.MaleCount ?? 0,
                request.FemaleCount ?? 0,
                request.CreatedByUserId);
        }

        // Age composition
        if (request.InfantCount.HasValue || request.ChildCount.HasValue ||
            request.MinorCount.HasValue || request.AdultCount.HasValue || request.ElderlyCount.HasValue)
        {
            household.UpdateAgeComposition(
                request.InfantCount ?? 0,
                request.ChildCount ?? 0,
                request.MinorCount ?? 0,
                request.AdultCount ?? 0,
                request.ElderlyCount ?? 0,
                request.CreatedByUserId);
        }

        // Vulnerability indicators
        if (request.PersonsWithDisabilitiesCount.HasValue || request.IsFemaleHeaded.HasValue ||
            request.WidowCount.HasValue || request.OrphanCount.HasValue || request.SingleParentCount.HasValue)
        {
            household.UpdateVulnerabilityIndicators(
                request.PersonsWithDisabilitiesCount ?? 0,
                request.IsFemaleHeaded ?? false,
                request.WidowCount ?? 0,
                request.OrphanCount ?? 0,
                request.SingleParentCount ?? 0,
                request.CreatedByUserId);
        }

        // Economic indicators
        if (request.EmployedPersonsCount.HasValue || request.UnemployedPersonsCount.HasValue ||
            !string.IsNullOrWhiteSpace(request.PrimaryIncomeSource) || request.MonthlyIncomeEstimate.HasValue)
        {
            household.UpdateEconomicIndicators(
                request.EmployedPersonsCount ?? 0,
                request.UnemployedPersonsCount ?? 0,
                request.PrimaryIncomeSource,
                request.MonthlyIncomeEstimate,
                request.CreatedByUserId);
        }

        // Displacement information
        if (request.IsDisplaced.HasValue || !string.IsNullOrWhiteSpace(request.OriginLocation) ||
            request.ArrivalDate.HasValue || !string.IsNullOrWhiteSpace(request.DisplacementReason))
        {
            household.UpdateDisplacementInfo(
                request.IsDisplaced ?? false,
                request.OriginLocation,
                request.ArrivalDate,
                request.DisplacementReason,
                request.CreatedByUserId);
        }

        // Link head of household person if provided
        if (request.HeadOfHouseholdPersonId.HasValue)
        {
            household.LinkHeadOfHousehold(request.HeadOfHouseholdPersonId.Value, request.CreatedByUserId);
        }

        // Special needs
        if (!string.IsNullOrWhiteSpace(request.SpecialNeeds))
        {
            household.UpdateSpecialNeeds(request.SpecialNeeds, request.CreatedByUserId);
        }

        // Notes
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            household.AddNotes(request.Notes, request.CreatedByUserId);
        }

        // Add to repository
        await _householdRepository.AddAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        // Map to DTO
        return _mapper.Map<HouseholdDto>(household);
    }
}
