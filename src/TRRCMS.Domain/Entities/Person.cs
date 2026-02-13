using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Person entity - represents individuals (owners, claimants, household members, contact persons)
/// إضافة شخص جديد
/// </summary>
public class Person : BaseAuditableEntity
{
    // ==================== PERSONAL IDENTIFICATION ====================

    /// <summary>
    /// Family/Last name in Arabic (الكنية)
    /// </summary>
    public string? FamilyNameArabic { get; private set; }

    /// <summary>
    /// First name in Arabic (الاسم الأول)
    /// </summary>
    public string? FirstNameArabic { get; private set; }

    /// <summary>
    /// Father's name in Arabic (اسم الأب)
    /// </summary>
    public string? FatherNameArabic { get; private set; }

    /// <summary>
    /// Mother's name in Arabic (الاسم الأم)
    /// </summary>
    public string? MotherNameArabic { get; private set; }

    /// <summary>
    /// National ID or identification number (الرقم الوطني)
    /// </summary>
    public string? NationalId { get; private set; }

    /// <summary>
    /// Date of birth (تاريخ الميلاد)
    /// Can store full date or year-only (stored as January 1st of that year)
    /// </summary>
    public DateTime? DateOfBirth { get; private set; }

    // ==================== CONTACT INFORMATION ====================

    /// <summary>
    /// Email address (البريد الالكتروني)
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Mobile phone number (رقم الموبايل)
    /// </summary>
    public string? MobileNumber { get; private set; }

    /// <summary>
    /// Landline phone number (رقم الهاتف)
    /// </summary>
    public string? PhoneNumber { get; private set; }

    // ==================== LEGACY FIELDS (for future expansion) ====================

    /// <summary>
    /// Full name in English (optional)
    /// </summary>
    public string? FullNameEnglish { get; private set; }

    /// <summary>
    /// Gender (الجنس)
    /// </summary>
    public Gender? Gender { get; private set; }

    /// <summary>
    /// Nationality (الجنسية)
    /// </summary>
    public Nationality? Nationality { get; private set; }

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
    /// Relationship to head of household (علاقة برب الأسرة)
    /// </summary>
    public RelationshipToHead? RelationshipToHead { get; private set; }

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
        FamilyNameArabic = null;
        FirstNameArabic = null;
        FatherNameArabic = null;
        PropertyRelations = new List<PersonPropertyRelation>();
        Evidences = new List<Evidence>();
    }

    /// <summary>
    /// Create new person with required Arabic names
    /// </summary>
    public static Person Create(
        string familyNameArabic,
        string firstNameArabic,
        string fatherNameArabic,
        string? motherNameArabic,
        Guid createdByUserId)
    {
        var person = new Person
        {
            FamilyNameArabic = familyNameArabic,
            FirstNameArabic = firstNameArabic,
            FatherNameArabic = fatherNameArabic,
            MotherNameArabic = motherNameArabic,
            IsContactPerson = false,
            HasIdentificationDocument = false
        };

        person.MarkAsCreated(createdByUserId);

        return person;
    }

    /// <summary>
    /// Create person with full info (for simplified API)
    /// </summary>
    public static Person CreateWithFullInfo(
        string? familyNameArabic,
        string? firstNameArabic,
        string? fatherNameArabic,
        string? motherNameArabic,
        string? nationalId,
        DateTime? dateOfBirth,
        Gender? gender,
        Nationality? nationality,
        string? email,
        string? mobileNumber,
        string? phoneNumber,
        Guid createdByUserId)
    {
        var person = new Person
        {
            FamilyNameArabic = familyNameArabic,
            FirstNameArabic = firstNameArabic,
            FatherNameArabic = fatherNameArabic,
            MotherNameArabic = motherNameArabic,
            NationalId = nationalId,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Nationality = nationality,
            Email = email,
            MobileNumber = mobileNumber,
            PhoneNumber = phoneNumber,
            IsContactPerson = false,
            HasIdentificationDocument = false
        };

        person.MarkAsCreated(createdByUserId);

        return person;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update basic info (simplified API)
    /// </summary>
    public void UpdateBasicInfo(
        string? familyNameArabic,
        string? firstNameArabic,
        string? fatherNameArabic,
        string? motherNameArabic,
        string? nationalId,
        DateTime? dateOfBirth,
        Gender? gender,
        Nationality? nationality,
        Guid modifiedByUserId)
    {
        FamilyNameArabic = familyNameArabic;
        FirstNameArabic = firstNameArabic;
        FatherNameArabic = fatherNameArabic;
        MotherNameArabic = motherNameArabic;
        NationalId = nationalId;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Nationality = nationality;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update contact info (simplified API)
    /// </summary>
    public void UpdateContactInfo(
        string? email,
        string? mobileNumber,
        string? phoneNumber,
        Guid modifiedByUserId)
    {
        Email = email;
        MobileNumber = mobileNumber;
        PhoneNumber = phoneNumber;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update identification details
    /// </summary>
    public void UpdateIdentification(
        string? nationalId,
        DateTime? dateOfBirth,
        Gender? gender,
        Nationality? nationality,
        Guid modifiedByUserId)
    {
        NationalId = nationalId;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Nationality = nationality;
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
        RelationshipToHead relationshipToHead,
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
    /// Set contact person flag
    /// </summary>
    public void SetAsContactPerson(bool isContact, Guid modifiedByUserId)
    {
        IsContactPerson = isContact;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Get full Arabic name (computed property for display)
    /// </summary>
    public string GetFullNameArabic()
    {
        var parts = new List<string> { FirstNameArabic, FatherNameArabic, FamilyNameArabic };
        return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    /// <summary>
    /// Calculate age based on date of birth
    /// </summary>
    public int? CalculateAge()
    {
        if (!DateOfBirth.HasValue)
            return null;

        var today = DateTime.UtcNow;
        var age = today.Year - DateOfBirth.Value.Year;

        // Subtract 1 if birthday hasn't occurred this year yet
        if (DateOfBirth.Value.Date > today.AddYears(-age))
            age--;

        return age;
    }
}
