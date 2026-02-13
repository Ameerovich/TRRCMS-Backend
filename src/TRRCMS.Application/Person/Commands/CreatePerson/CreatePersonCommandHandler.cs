using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Persons.Commands.CreatePerson;

/// <summary>
/// Handler for CreatePersonCommand
/// </summary>
public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, PersonDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreatePersonCommandHandler(
        IPersonRepository personRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonDto> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Create person entity using factory method
        var person = Person.CreateWithFullInfo(
            familyNameArabic: request.FamilyNameArabic,
            firstNameArabic: request.FirstNameArabic,
            fatherNameArabic: request.FatherNameArabic,
            motherNameArabic: request.MotherNameArabic,
            nationalId: request.NationalId,
            dateOfBirth: request.DateOfBirth,
            gender: request.Gender,
            nationality: request.Nationality,
            email: request.Email,
            mobileNumber: request.MobileNumber,
            phoneNumber: request.PhoneNumber,
            createdByUserId: currentUserId);

        // Save to repository
        await _personRepository.AddAsync(person, cancellationToken);
        await _personRepository.SaveChangesAsync(cancellationToken);

        // Map to DTO and return
        return _mapper.Map<PersonDto>(person);
    }
}
