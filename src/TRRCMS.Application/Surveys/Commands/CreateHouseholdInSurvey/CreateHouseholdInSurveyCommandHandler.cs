using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;

/// <summary>
/// Handler for CreateHouseholdInSurveyCommand
/// Creates household and links it to property unit in survey context
/// </summary>
public class CreateHouseholdInSurveyCommandHandler : IRequestHandler<CreateHouseholdInSurveyCommand, HouseholdDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public CreateHouseholdInSurveyCommandHandler(
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

    public async Task<HouseholdDto> Handle(CreateHouseholdInSurveyCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("You can only create households for your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot create households for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Get and validate property unit
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId, cancellationToken);
        if (propertyUnit == null)
        {
            throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
        }

        // Verify property unit belongs to survey's building
        if (propertyUnit.BuildingId != survey.BuildingId)
        {
            throw new ValidationException(
                $"Property unit does not belong to survey building. " +
                $"Property unit building: {propertyUnit.BuildingId}, Survey building: {survey.BuildingId}");
        }

        // Create household entity
        var household = Household.Create(
            propertyUnitId: request.PropertyUnitId,
            headOfHouseholdName: request.HeadOfHouseholdName,
            householdSize: request.HouseholdSize,
            createdByUserId: currentUserId
        );

        // Update gender composition if provided
        if (request.MaleCount.HasValue || request.FemaleCount.HasValue)
        {
            household.UpdateGenderComposition(
                maleCount: request.MaleCount ?? 0,
                femaleCount: request.FemaleCount ?? 0,
                modifiedByUserId: currentUserId
            );
        }

        // Update age composition if provided
        if (request.InfantCount.HasValue || request.ChildCount.HasValue ||
            request.MinorCount.HasValue || request.AdultCount.HasValue || request.ElderlyCount.HasValue)
        {
            household.UpdateAgeComposition(
                infantCount: request.InfantCount ?? 0,
                childCount: request.ChildCount ?? 0,
                minorCount: request.MinorCount ?? 0,
                adultCount: request.AdultCount ?? 0,
                elderlyCount: request.ElderlyCount ?? 0,
                modifiedByUserId: currentUserId
            );
        }

        // Update vulnerability indicators if provided
        if (request.PersonsWithDisabilitiesCount.HasValue || request.IsFemaleHeaded.HasValue ||
            request.WidowCount.HasValue || request.OrphanCount.HasValue || request.SingleParentCount.HasValue)
        {
            household.UpdateVulnerabilityIndicators(
                personsWithDisabilitiesCount: request.PersonsWithDisabilitiesCount ?? 0,
                isFemaleHeaded: request.IsFemaleHeaded ?? false,
                widowCount: request.WidowCount ?? 0,
                orphanCount: request.OrphanCount ?? 0,
                singleParentCount: request.SingleParentCount ?? 0,
                modifiedByUserId: currentUserId
            );
        }

        // Update economic indicators if provided
        if (request.EmployedPersonsCount.HasValue || request.UnemployedPersonsCount.HasValue ||
            !string.IsNullOrWhiteSpace(request.PrimaryIncomeSource) || request.MonthlyIncomeEstimate.HasValue)
        {
            household.UpdateEconomicIndicators(
                employedCount: request.EmployedPersonsCount ?? 0,
                unemployedCount: request.UnemployedPersonsCount ?? 0,
                primaryIncomeSource: request.PrimaryIncomeSource,
                monthlyIncomeEstimate: request.MonthlyIncomeEstimate,
                modifiedByUserId: currentUserId
            );
        }

        // Update displacement information if provided
        if (request.IsDisplaced.HasValue)
        {
            household.UpdateDisplacementInfo(
                isDisplaced: request.IsDisplaced.Value,
                originLocation: request.OriginLocation,
                arrivalDate: request.ArrivalDate,
                displacementReason: request.DisplacementReason,
                modifiedByUserId: currentUserId
            );
        }

        // Add notes if provided
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            household.AddNotes(request.Notes, currentUserId);
        }

        // Update special needs if provided
        if (!string.IsNullOrWhiteSpace(request.SpecialNeeds))
        {
            household.UpdateSpecialNeeds(request.SpecialNeeds, currentUserId);
        }

        // Save household
        await _householdRepository.AddAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created household for {request.HeadOfHouseholdName} in survey {survey.ReferenceCode} at property unit {propertyUnit.UnitIdentifier}",
            entityType: "Household",
            entityId: household.Id,
            entityIdentifier: request.HeadOfHouseholdName,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                household.HeadOfHouseholdName,
                household.HouseholdSize,
                request.PropertyUnitId,
                PropertyUnitIdentifier = propertyUnit.UnitIdentifier,
                request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode
            }),
            changedFields: "New Household in Survey",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<HouseholdDto>(household);

        // Calculate computed properties
        result.DependencyRatio = household.CalculateDependencyRatio();
        result.IsVulnerable = household.IsVulnerable();

        return result;
    }
}