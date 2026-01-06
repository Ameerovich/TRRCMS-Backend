namespace TRRCMS.Application.Persons.Dtos
{
    public class PersonDto
    {
        public Guid Id { get; set; }

        // Arabic Names (Primary)
        public string FirstNameArabic { get; set; } = string.Empty;
        public string FatherNameArabic { get; set; } = string.Empty;
        public string FamilyNameArabic { get; set; } = string.Empty;
        public string? MotherNameArabic { get; set; }

        // English Name (Optional)
        public string? FullNameEnglish { get; set; }

        // Identification
        public string? NationalId { get; set; }

        // Demographics
        public int? YearOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }

        // Contact Information
        public string? PrimaryPhoneNumber { get; set; }
        public string? SecondaryPhoneNumber { get; set; }
        public bool IsContactPerson { get; set; }

        // Household Relationship
        public Guid? HouseholdId { get; set; }
        public string? RelationshipToHead { get; set; }

        // Document Flag
        public bool HasIdentificationDocument { get; set; }

        // Audit fields 
        public DateTime CreatedAtUtc { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? LastModifiedAtUtc { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
        public Guid? DeletedBy { get; set; }

        // Computed property for display
        public string FullNameArabic => $"{FirstNameArabic} {FatherNameArabic} {FamilyNameArabic}";
        public int? Age => YearOfBirth.HasValue ? DateTime.UtcNow.Year - YearOfBirth.Value : null;
    }
}