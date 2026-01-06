using System.Security.Policy;
using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Person entity - represents individuals (owners, claimants, household members, contact persons)
/// </summary>
public class Person : BaseAuditableEntity
{
    // ==================== PERSONAL IDENTIFICATION ====================

    /// <summary>
    /// First name in Arabic (الاسم الأول)
    /// </summary>
    public string FirstNameArabic { get; private set; }

    /// <summary>
    /// Father's name in Arabic (اسم الأب)
    /// </summary>
    public string FatherNameArabic { get; private set; }

    /// <summary>
    /// Grandfather's name in Arabic (اسم الجد)
    /// </summary>
    public string FamilyNameArabic { get; private set; }

    /// <summary>
    /// Mother's name in Arabic (اسم الأم)
    /// </summary>
    public string? MotherNameArabic { get; private set; }

    /// <summary>
    /// Full name in English (optional)
    /// </summary>
    public string? FullNameEnglish { get; private set; }

    /// <summary>
    /// National ID or identification number
    /// </summary>
    public string? NationalId { get; private set; }

    // ==================== BIRTH & DEMOGRAPHICS ====================

    /// <summary>
    /// Year of birth (integer, not full date)
    /// </summary>
    public int? YearOfBirth { get; private set; }

    /// <summary>
    /// Gender (controlled vocabulary: M/F)
    /// </summary>
    public string? Gender { get; private set; }

    /// <summary>
    /// Nationality (controlled vocabulary)
    /// </summary>
    public string? Nationality { get; private set; }

    // ==================== CONTACT INFORMATION ====================

    /// <summary>
    /// Primary phone number
    /// </summary>
    public string? PrimaryPhoneNumber { get; private set; }

    /// <summary>
    /// Secondary phone number (optional)
    /// </summary>
    public string? SecondaryPhoneNumber { get; private set; }

    /// <summary>
    /// Indicates if this person is the main contact person
    /// </summary>
    public bool IsContactPerson { get; private set; }

    // ==================== HOUSEHOLD RELATIONSHIP ====================

    /// <summary>
    /// Foreign key to household (nullable for non-household persons)
    /// </summary>
    public Guid? HouseholdId { get; private set; }

    /// <summary>
    /// Relationship to head of household
    /// </summary>
    public string? RelationshipToHead { get; private set; }

    // ==================== IDENTIFICATION DOCUMENTS ====================

    /// <summary>
    /// Flag indicating if personal identification document was uploaded
    /// </summary>
    public bool HasIdentificationDocument { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Household this person belongs to (if applicable)
    /// </summary>
    public virtual Household? Household { get; private set; }

    /// <summary>
    /// Relations between this person and property units
    /// </summary>
    public virtual ICollection<PersonPropertyRelation> PropertyRelations { get; private set; }

    /// <summary>
    /// Evidence/documents attached to this person
    /// </summary>
    public virtual ICollection<Evidence> Evidences { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Person() : base()
    {
        FirstNameArabic = string.Empty;
        FamilyNameArabic = string.Empty;
        FatherNameArabic = string.Empty;
        PropertyRelations = new List<PersonPropertyRelation>();
        Evidences = new List<Evidence>();
    }

    /// <summary>
    /// Create new person with required Arabic names
    /// </summary>
    public static Person Create(
        string firstNameArabic,
        string fatherNameArabic,
        string LastNameArabic,
        string? motherNameArabic,
        Guid createdByUserId)
    {
        var person = new Person
        {
            FirstNameArabic = firstNameArabic,
            FatherNameArabic = fatherNameArabic,
            FamilyNameArabic = LastNameArabic,
            MotherNameArabic = motherNameArabic,
            IsContactPerson = false,
            HasIdentificationDocument = false
        };

        person.MarkAsCreated(createdByUserId);

        return person;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update identification details
    /// </summary>
    public void UpdateIdentification(
        string? nationalId,
        int? yearOfBirth,
        string? gender,
        string? nationality,
        Guid modifiedByUserId)
    {
        NationalId = nationalId;
        YearOfBirth = yearOfBirth;
        Gender = gender;
        Nationality = nationality;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update contact information
    /// </summary>
    public void UpdateContactInfo(
        string? primaryPhone,
        string? secondaryPhone,
        bool isContactPerson,
        Guid modifiedByUserId)
    {
        PrimaryPhoneNumber = primaryPhone;
        SecondaryPhoneNumber = secondaryPhone;
        IsContactPerson = isContactPerson;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update English name
    /// </summary>
    public void UpdateEnglishName(string fullNameEnglish, Guid modifiedByUserId)
    {
        FullNameEnglish = fullNameEnglish;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Assign to household with relationship
    /// </summary>
    public void AssignToHousehold(
        Guid householdId,
        string relationshipToHead,
        Guid modifiedByUserId)
    {
        HouseholdId = householdId;
        RelationshipToHead = relationshipToHead;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Remove from household
    /// </summary>
    public void RemoveFromHousehold(Guid modifiedByUserId)
    {
        HouseholdId = null;
        RelationshipToHead = null;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark that identification document was uploaded
    /// </summary>
    public void MarkIdentificationDocumentUploaded(Guid modifiedByUserId)
    {
        HasIdentificationDocument = true;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Get full Arabic name (computed property for display)
    /// </summary>
    public string GetFullNameArabic()
    {
        var parts = new List<string> { FirstNameArabic, FatherNameArabic, FamilyNameArabic };

        var fullName = string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        if (!string.IsNullOrWhiteSpace(MotherNameArabic))
            fullName += $" ({MotherNameArabic})";

        return fullName;
    }

    /// <summary>
    /// Calculate approximate age based on year of birth
    /// </summary>
    public int? CalculateAge()
    {
        if (!YearOfBirth.HasValue)
            return null;

        return DateTime.UtcNow.Year - YearOfBirth.Value;
    }
}