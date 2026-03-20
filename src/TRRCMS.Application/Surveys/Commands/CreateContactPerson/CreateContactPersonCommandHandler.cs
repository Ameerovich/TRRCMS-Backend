using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.CreateContactPerson;

/// <summary>
/// Handler for CreateContactPersonCommand
/// Creates a contact person and links to the survey (no household required)
/// </summary>
public class CreateContactPersonCommandHandler : IRequestHandler<CreateContactPersonCommand, PersonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateContactPersonCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonDto> Handle(CreateContactPersonCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate survey exists
        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        // Verify ownership
        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_EditAll))
                throw new UnauthorizedAccessException("You can only add persons to your own surveys");
        }

        // Only draft surveys can be modified
        survey.EnsureCanModify();

        // Check NationalId uniqueness
        if (!string.IsNullOrWhiteSpace(request.NationalId))
        {
            var existingPerson = await _unitOfWork.Persons.GetByNationalIdAsync(request.NationalId, cancellationToken);
            if (existingPerson != null)
                throw new ConflictException(
                    $"A person with National ID '{request.NationalId}' already exists.",
                    _mapper.Map<PersonDto>(existingPerson));
        }

        // Create person
        var person = Person.CreateWithFullInfo(
            familyNameArabic: request.FamilyNameArabic,
            firstNameArabic: request.FirstNameArabic,
            fatherNameArabic: request.FatherNameArabic,
            motherNameArabic: request.MotherNameArabic,
            nationalId: request.NationalId,
            dateOfBirth: request.DateOfBirth,
            gender: request.Gender.HasValue ? (Gender)request.Gender.Value : null,
            nationality: request.Nationality.HasValue ? (Nationality)request.Nationality.Value : null,
            email: request.Email,
            mobileNumber: request.MobileNumber,
            phoneNumber: request.PhoneNumber,
            createdByUserId: currentUserId);

        // Mark as contact person
        person.SetAsContactPerson(true, currentUserId);

        // Link to survey
        survey.SetContactPerson(person.Id, person.GetContactPersonFullName(), currentUserId);

        // Save
        await _unitOfWork.Persons.AddAsync(person, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PersonDto>(person);
    }
}
