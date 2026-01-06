using MediatR;
using AutoMapper;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Persons.Commands.CreatePerson
{
    public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, PersonDto>
    {
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;

        public CreatePersonCommandHandler(IPersonRepository personRepository, IMapper mapper)
        {
            _personRepository = personRepository;
            _mapper = mapper;
        }

        public async Task<PersonDto> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            // Validate required Arabic names
            if (string.IsNullOrWhiteSpace(request.FirstNameArabic))
                throw new ArgumentException("FirstNameArabic is required.");

            if (string.IsNullOrWhiteSpace(request.FatherNameArabic))
                throw new ArgumentException("FatherNameArabic is required.");

            if (string.IsNullOrWhiteSpace(request.LastNameArabic))
                throw new ArgumentException("LastNameArabic is required.");

            // Validate that NationalId doesn't already exist (if provided)
            if (!string.IsNullOrWhiteSpace(request.NationalId))
            {
                var exists = await _personRepository.NationalIdExistsAsync(request.NationalId, cancellationToken);
                if (exists)
                {
                    throw new InvalidOperationException($"A person with NationalId '{request.NationalId}' already exists.");
                }
            }

            // Validate year of birth (if provided)
            if (request.YearOfBirth.HasValue)
            {
                var currentYear = DateTime.UtcNow.Year;
                if (request.YearOfBirth < 1900 || request.YearOfBirth > currentYear)
                {
                    throw new ArgumentException($"YearOfBirth must be between 1900 and {currentYear}.");
                }
            }

            // Create person using factory method
            var person = Person.Create(
                firstNameArabic: request.FirstNameArabic,
                fatherNameArabic: request.FatherNameArabic,
                LastNameArabic: request.LastNameArabic,
                motherNameArabic: request.MotherNameArabic,
                createdByUserId: request.CreatedByUserId
            );

            // Update optional fields using domain methods
            if (!string.IsNullOrWhiteSpace(request.NationalId) ||
                request.YearOfBirth.HasValue ||
                !string.IsNullOrWhiteSpace(request.Gender) ||
                !string.IsNullOrWhiteSpace(request.Nationality))
            {
                person.UpdateIdentification(
                    nationalId: request.NationalId,
                    yearOfBirth: request.YearOfBirth,
                    gender: request.Gender,
                    nationality: request.Nationality,
                    modifiedByUserId: request.CreatedByUserId
                );
            }

            if (!string.IsNullOrWhiteSpace(request.PrimaryPhoneNumber) ||
                !string.IsNullOrWhiteSpace(request.SecondaryPhoneNumber) ||
                request.IsContactPerson)
            {
                person.UpdateContactInfo(
                    primaryPhone: request.PrimaryPhoneNumber,
                    secondaryPhone: request.SecondaryPhoneNumber,
                    isContactPerson: request.IsContactPerson,
                    modifiedByUserId: request.CreatedByUserId
                );
            }

            if (!string.IsNullOrWhiteSpace(request.FullNameEnglish))
            {
                person.UpdateEnglishName(
                    fullNameEnglish: request.FullNameEnglish,
                    modifiedByUserId: request.CreatedByUserId
                );
            }

            if (request.HouseholdId.HasValue)
            {
                person.AssignToHousehold(
                    householdId: request.HouseholdId.Value,
                    relationshipToHead: request.RelationshipToHead,
                    modifiedByUserId: request.CreatedByUserId
                );
            }

            if (request.HasIdentificationDocument)
            {
                person.MarkIdentificationDocumentUploaded(modifiedByUserId: request.CreatedByUserId);
            }

            // Add person to repository
            var createdPerson = await _personRepository.AddAsync(person, cancellationToken);

            // SAVE CHANGES 
            await _personRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<PersonDto>(createdPerson);
        }
    }
}