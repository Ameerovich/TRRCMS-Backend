using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Commands.CreatePerson
{
    public class CreatePersonCommand : IRequest<PersonDto>
    {
        // Required Arabic Names
        public string FirstNameArabic { get; set; } = string.Empty;
        public string FatherNameArabic { get; set; } = string.Empty;
        public string LastNameArabic { get; set; } = string.Empty;  // ← Keep this to match Create method parameter

        // Optional Mother Name
        public string? MotherNameArabic { get; set; }

        // Optional English Full Name
        public string? FullNameEnglish { get; set; }

        // Optional Identification
        public string? NationalId { get; set; }
        public int? YearOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }

        // Optional Contact Info
        public string? PrimaryPhoneNumber { get; set; }
        public string? SecondaryPhoneNumber { get; set; }
        public bool IsContactPerson { get; set; }

        // Household Relationship (Optional)
        public Guid? HouseholdId { get; set; }
        public string? RelationshipToHead { get; set; }

        // Document Flag
        public bool HasIdentificationDocument { get; set; }

        // User who is creating this person
        public Guid CreatedByUserId { get; set; }
    }
}