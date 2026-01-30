using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Surveys.Commands.AddPersonToHousehold;

/// <summary>
/// Handler for AddPersonToHouseholdCommand
/// </summary>
public class AddPersonToHouseholdCommandHandler : IRequestHandler<AddPersonToHouseholdCommand, PersonDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public AddPersonToHouseholdCommandHandler(
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        IPersonRepository personRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonDto> Handle(AddPersonToHouseholdCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate survey exists and user has access
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership
        if (survey.FieldCollectorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only add persons to your own surveys");
        }

        // Validate household exists
        var household = await _householdRepository.GetByIdAsync(request.HouseholdId, cancellationToken);
        if (household == null)
        {
            throw new NotFoundException($"Household with ID {request.HouseholdId} not found");
        }

        // Create person entity
        var person = Person.CreateWithFullInfo(
            familyNameArabic: request.FamilyNameArabic,
            firstNameArabic: request.FirstNameArabic,
            fatherNameArabic: request.FatherNameArabic,
            motherNameArabic: request.MotherNameArabic,
            nationalId: request.NationalId,
            yearOfBirth: request.YearOfBirth,
            email: request.Email,
            mobileNumber: request.MobileNumber,
            phoneNumber: request.PhoneNumber,
            createdByUserId: currentUserId);

        // Assign to household
        if (!string.IsNullOrEmpty(request.RelationshipToHead))
        {
            person.AssignToHousehold(request.HouseholdId, request.RelationshipToHead, currentUserId);
        }
        else
        {
            person.AssignToHousehold(request.HouseholdId, "Member", currentUserId);
        }

        // Save to repository
        await _personRepository.AddAsync(person, cancellationToken);
        await _personRepository.SaveChangesAsync(cancellationToken);

        // Map to DTO and return
        return _mapper.Map<PersonDto>(person);
    }
}
