using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.SetHouseholdHead;

/// <summary>
/// Handler for SetHouseholdHeadCommand
/// Designates a person as the head of household
/// </summary>
public class SetHouseholdHeadCommandHandler : IRequestHandler<SetHouseholdHeadCommand, HouseholdDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public SetHouseholdHeadCommandHandler(
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        IPersonRepository personRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<HouseholdDto> Handle(SetHouseholdHeadCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("You can only set household head for your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot modify households for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Get and validate household
        var household = await _householdRepository.GetByIdAsync(request.HouseholdId, cancellationToken);
        if (household == null)
        {
            throw new NotFoundException($"Household with ID {request.HouseholdId} not found");
        }

        // Get and validate person
        var person = await _personRepository.GetByIdAsync(request.PersonId, cancellationToken);
        if (person == null)
        {
            throw new NotFoundException($"Person with ID {request.PersonId} not found");
        }

        // Verify person belongs to this household
        if (person.HouseholdId != request.HouseholdId)
        {
            throw new ValidationException($"Person does not belong to this household. Person's household ID: {person.HouseholdId}, Requested household ID: {request.HouseholdId}");
        }

        // Store old head for audit
        var oldHeadPersonId = household.HeadOfHouseholdPersonId;

        // Link person as head of household
        household.LinkHeadOfHousehold(person.Id, currentUserId);

        // Save changes
        await _householdRepository.UpdateAsync(household, cancellationToken);
        await _householdRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Set {person.GetFullNameArabic()} as head of household {household.HeadOfHouseholdName} in survey {survey.ReferenceCode}",
            entityType: "Household",
            entityId: household.Id,
            entityIdentifier: household.HeadOfHouseholdName,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { HeadOfHouseholdPersonId = oldHeadPersonId }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new { HeadOfHouseholdPersonId = person.Id, HeadName = person.GetFullNameArabic() }),
            changedFields: "HeadOfHouseholdPersonId",
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