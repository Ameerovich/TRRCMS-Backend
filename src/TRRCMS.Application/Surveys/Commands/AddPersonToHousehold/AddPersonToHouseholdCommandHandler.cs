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
/// </summary>
public class AddPersonToHouseholdCommandHandler : IRequestHandler<AddPersonToHouseholdCommand, PersonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public AddPersonToHouseholdCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonDto> Handle(AddPersonToHouseholdCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate survey exists and user has access
        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken);
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
        var household = await _unitOfWork.Households.GetByIdAsync(request.HouseholdId, cancellationToken);
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
            dateOfBirth: request.DateOfBirth,
            gender: request.Gender.HasValue ? (Gender)request.Gender.Value : (Gender?)null,
            nationality: request.Nationality.HasValue ? (Nationality)request.Nationality.Value : (Nationality?)null,
            email: request.Email,
            mobileNumber: request.MobileNumber,
            phoneNumber: request.PhoneNumber,
            createdByUserId: currentUserId);

        // Assign to household
        if (request.RelationshipToHead.HasValue)
        {
            person.AssignToHousehold(request.HouseholdId, (RelationshipToHead)request.RelationshipToHead.Value, currentUserId);
        }

        // Save to repository
        await _unitOfWork.Persons.AddAsync(person, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO and return
        return _mapper.Map<PersonDto>(person);
    }
}
