using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Commands.UpdatePerson;

/// <summary>
/// Handler for UpdatePersonCommand
/// </summary>
public class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand, PersonDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdatePersonCommandHandler(
        IPersonRepository personRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonDto> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get existing person
        var person = await _personRepository.GetByIdAsync(request.Id, cancellationToken);
        if (person == null)
        {
            throw new NotFoundException($"Person with ID {request.Id} not found");
        }

        // Update basic info (only if provided)
        person.UpdateBasicInfo(
            familyNameArabic: request.FamilyNameArabic ?? person.FamilyNameArabic,
            firstNameArabic: request.FirstNameArabic ?? person.FirstNameArabic,
            fatherNameArabic: request.FatherNameArabic ?? person.FatherNameArabic,
            motherNameArabic: request.MotherNameArabic ?? person.MotherNameArabic,
            nationalId: request.NationalId ?? person.NationalId,
            yearOfBirth: request.YearOfBirth ?? person.YearOfBirth,
            modifiedByUserId: currentUserId);

        // Update contact info (only if any contact field provided)
        if (request.Email != null || request.MobileNumber != null || request.PhoneNumber != null)
        {
            person.UpdateContactInfo(
                email: request.Email ?? person.Email,
                mobileNumber: request.MobileNumber ?? person.MobileNumber,
                phoneNumber: request.PhoneNumber ?? person.PhoneNumber,
                modifiedByUserId: currentUserId);
        }

        // Save changes
        await _personRepository.UpdateAsync(person, cancellationToken);
        await _personRepository.SaveChangesAsync(cancellationToken);

        // Map to DTO and return
        return _mapper.Map<PersonDto>(person);
    }
}
