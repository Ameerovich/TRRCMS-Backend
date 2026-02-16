using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdatePersonInSurvey;

/// <summary>
/// Handler for UpdatePersonInSurveyCommand
/// Updates person details in the context of a survey
/// </summary>
public class UpdatePersonInSurveyCommandHandler : IRequestHandler<UpdatePersonInSurveyCommand, PersonDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdatePersonInSurveyCommandHandler(
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

    public async Task<PersonDto> Handle(UpdatePersonInSurveyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate survey exists and user has access
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only update persons in your own surveys");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot update persons for survey in {survey.Status} status. Only Draft surveys can be modified.");

        // Validate household exists
        var household = await _householdRepository.GetByIdAsync(request.HouseholdId, cancellationToken)
            ?? throw new NotFoundException($"Household with ID {request.HouseholdId} not found");

        // Verify household belongs to survey's building
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);
        if (propertyUnit == null || propertyUnit.BuildingId != survey.BuildingId)
            throw new ValidationException("Household does not belong to this survey's building");

        // Get person
        var person = await _personRepository.GetByIdAsync(request.PersonId, cancellationToken)
            ?? throw new NotFoundException($"Person with ID {request.PersonId} not found");

        // Verify person belongs to the specified household
        if (person.HouseholdId != request.HouseholdId)
            throw new ValidationException($"Person {request.PersonId} does not belong to household {request.HouseholdId}");

        // Capture old values for audit
        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            person.FamilyNameArabic,
            person.FirstNameArabic,
            person.FatherNameArabic,
            person.MotherNameArabic,
            person.NationalId,
            person.DateOfBirth,
            Gender = person.Gender?.ToString(),
            Nationality = person.Nationality?.ToString(),
            person.Email,
            person.MobileNumber,
            person.PhoneNumber,
            RelationshipToHead = person.RelationshipToHead?.ToString()
        });

        // Update basic info (use existing values for fields not provided)
        person.UpdateBasicInfo(
            familyNameArabic: request.FamilyNameArabic ?? person.FamilyNameArabic,
            firstNameArabic: request.FirstNameArabic ?? person.FirstNameArabic,
            fatherNameArabic: request.FatherNameArabic ?? person.FatherNameArabic,
            motherNameArabic: request.MotherNameArabic ?? person.MotherNameArabic,
            nationalId: request.NationalId ?? person.NationalId,
            dateOfBirth: request.DateOfBirth ?? person.DateOfBirth,
            gender: request.Gender.HasValue ? (Gender)request.Gender.Value : person.Gender,
            nationality: request.Nationality.HasValue ? (Nationality)request.Nationality.Value : person.Nationality,
            modifiedByUserId: currentUserId);

        // Update contact info if any contact field is provided
        if (request.Email != null || request.MobileNumber != null || request.PhoneNumber != null)
        {
            person.UpdateContactInfo(
                email: request.Email ?? person.Email,
                mobileNumber: request.MobileNumber ?? person.MobileNumber,
                phoneNumber: request.PhoneNumber ?? person.PhoneNumber,
                modifiedByUserId: currentUserId);
        }

        // Update relationship to head if provided
        if (request.RelationshipToHead.HasValue)
        {
            person.AssignToHousehold(request.HouseholdId, (RelationshipToHead)request.RelationshipToHead.Value, currentUserId);
        }

        // Save changes
        await _personRepository.UpdateAsync(person, cancellationToken);
        await _personRepository.SaveChangesAsync(cancellationToken);

        // Build changed fields list
        var changedFields = new List<string>();
        if (request.FamilyNameArabic != null) changedFields.Add("FamilyNameArabic");
        if (request.FirstNameArabic != null) changedFields.Add("FirstNameArabic");
        if (request.FatherNameArabic != null) changedFields.Add("FatherNameArabic");
        if (request.MotherNameArabic != null) changedFields.Add("MotherNameArabic");
        if (request.NationalId != null) changedFields.Add("NationalId");
        if (request.DateOfBirth.HasValue) changedFields.Add("DateOfBirth");
        if (request.Gender.HasValue) changedFields.Add("Gender");
        if (request.Nationality.HasValue) changedFields.Add("Nationality");
        if (request.Email != null) changedFields.Add("Email");
        if (request.MobileNumber != null) changedFields.Add("MobileNumber");
        if (request.PhoneNumber != null) changedFields.Add("PhoneNumber");
        if (request.RelationshipToHead.HasValue) changedFields.Add("RelationshipToHead");

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated person {person.GetFullNameArabic()} in survey {survey.ReferenceCode}",
            entityType: "Person",
            entityId: person.Id,
            entityIdentifier: person.GetFullNameArabic(),
            oldValues: oldValues,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                person.FamilyNameArabic,
                person.FirstNameArabic,
                person.FatherNameArabic,
                person.MotherNameArabic,
                person.NationalId,
                person.DateOfBirth,
                Gender = person.Gender?.ToString(),
                Nationality = person.Nationality?.ToString(),
                person.Email,
                person.MobileNumber,
                person.PhoneNumber,
                RelationshipToHead = person.RelationshipToHead?.ToString()
            }),
            changedFields: string.Join(", ", changedFields),
            cancellationToken: cancellationToken);

        // Map to DTO and return
        return _mapper.Map<PersonDto>(person);
    }
}
