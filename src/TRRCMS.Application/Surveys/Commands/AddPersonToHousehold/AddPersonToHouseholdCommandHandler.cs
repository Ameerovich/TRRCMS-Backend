using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.AddPersonToHousehold;

/// <summary>
/// Handler for AddPersonToHouseholdCommand
/// Adds a person to a household in survey context
/// </summary>
public class AddPersonToHouseholdCommandHandler : IRequestHandler<AddPersonToHouseholdCommand, PersonDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public AddPersonToHouseholdCommandHandler(
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        IPersonRepository personRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonDto> Handle(AddPersonToHouseholdCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("You can only add persons to households in your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot add persons to households for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Get and validate household
        var household = await _householdRepository.GetByIdAsync(request.HouseholdId, cancellationToken);
        if (household == null)
        {
            throw new NotFoundException($"Household with ID {request.HouseholdId} not found");
        }

        // Get property unit for the household and verify it belongs to survey's building
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);
        if (propertyUnit == null)
        {
            throw new NotFoundException($"Property unit with ID {household.PropertyUnitId} not found");
        }

        if (propertyUnit.BuildingId != survey.BuildingId)
        {
            throw new ValidationException("Household does not belong to the survey's building");
        }

        // Create person entity
        var person = Person.Create(
            firstNameArabic: request.FirstNameArabic,
            fatherNameArabic: request.FatherNameArabic,
            LastNameArabic: request.FamilyNameArabic,
            motherNameArabic: request.MotherNameArabic,
            createdByUserId: currentUserId
        );

        // Update identification if provided
        if (!string.IsNullOrWhiteSpace(request.NationalId) || request.YearOfBirth.HasValue ||
            !string.IsNullOrWhiteSpace(request.Gender) || !string.IsNullOrWhiteSpace(request.Nationality))
        {
            person.UpdateIdentification(
                nationalId: request.NationalId,
                yearOfBirth: request.YearOfBirth,
                gender: request.Gender,
                nationality: request.Nationality,
                modifiedByUserId: currentUserId
            );
        }

        // Update English name if provided
        if (!string.IsNullOrWhiteSpace(request.FullNameEnglish))
        {
            person.UpdateEnglishName(request.FullNameEnglish, currentUserId);
        }

        // Update contact information if provided
        if (!string.IsNullOrWhiteSpace(request.PrimaryPhoneNumber) ||
            !string.IsNullOrWhiteSpace(request.SecondaryPhoneNumber) ||
            request.IsContactPerson.HasValue)
        {
            person.UpdateContactInfo(
                primaryPhone: request.PrimaryPhoneNumber,
                secondaryPhone: request.SecondaryPhoneNumber,
                isContactPerson: request.IsContactPerson ?? false,
                modifiedByUserId: currentUserId
            );
        }

        // Assign to household
        person.AssignToHousehold(
            householdId: request.HouseholdId,
            relationshipToHead: request.RelationshipToHead ?? "Member",
            modifiedByUserId: currentUserId
        );

        // Save person
        await _personRepository.AddAsync(person, cancellationToken);
        await _personRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Added person {person.GetFullNameArabic()} to household {household.HeadOfHouseholdName} in survey {survey.ReferenceCode}",
            entityType: "Person",
            entityId: person.Id,
            entityIdentifier: person.GetFullNameArabic(),
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                person.FirstNameArabic,
                person.FatherNameArabic,
                person.FamilyNameArabic,
                person.RelationshipToHead,
                request.HouseholdId,
                HouseholdHead = household.HeadOfHouseholdName,
                request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode
            }),
            changedFields: "New Person in Household",
            cancellationToken: cancellationToken
        );

        // Map to DTO
        var result = _mapper.Map<PersonDto>(person);

        return result;
    }
}