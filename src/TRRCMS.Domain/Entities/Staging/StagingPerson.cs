using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for Person records from .uhc packages.
/// Mirrors the <see cref="Person"/> production entity in an isolated staging area.
/// Central to duplicate detection per FSD FR-D-5 (Person Matching):
/// - NationalId exact match
/// - Arabic name Levenshtein similarity
/// - Phone number match
/// - Year of birth + gender composite
/// 
/// Referenced in UC-003 Stage 2 and UC-008 (Resolve Person Duplicates).
/// </summary>
public class StagingPerson : BaseStagingEntity
{
    // ==================== NAME COMPONENTS ====================

    /// <summary>Family name in Arabic (اسم العائلة).</summary>
    public string FamilyNameArabic { get; private set; }

    /// <summary>First name in Arabic (الاسم الأول).</summary>
    public string FirstNameArabic { get; private set; }

    /// <summary>Father's name in Arabic (اسم الأب).</summary>
    public string FatherNameArabic { get; private set; }

    /// <summary>Mother's name in Arabic (اسم الأم).</summary>
    public string? MotherNameArabic { get; private set; }

    // ==================== IDENTIFICATION ====================

    /// <summary>
    /// National ID number — primary key for duplicate detection (FR-D-5, §12.2.4).
    /// Exact match on this field produces a high-confidence duplicate.
    /// </summary>
    public string? NationalId { get; private set; }

    /// <summary>Year of birth (تاريخ الميلاد) — from command, optional. Used in duplicate detection composite.</summary>
    public int? YearOfBirth { get; private set; }

    // ==================== CONTACT ====================

    /// <summary>Email address.</summary>
    public string? Email { get; private set; }

    /// <summary>Mobile phone number.</summary>
    public string? MobileNumber { get; private set; }

    /// <summary>Landline phone number.</summary>
    public string? PhoneNumber { get; private set; }

    // ==================== ADDITIONAL DETAILS ====================

    /// <summary>Full name in English (transliteration).</summary>
    public string? FullNameEnglish { get; private set; }

    /// <summary>Gender (stored as string to accommodate .uhc variations).</summary>
    public string? Gender { get; private set; }

    /// <summary>Nationality.</summary>
    public string? Nationality { get; private set; }

    // ==================== HOUSEHOLD LINK ====================

    /// <summary>
    /// Original Household UUID from .uhc — not a FK to production Households.
    /// Used for intra-batch household structure validation.
    /// </summary>
    public Guid? OriginalHouseholdId { get; private set; }

    /// <summary>Relationship to the head of household.</summary>
    public string? RelationshipToHead { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>EF Core constructor.</summary>
    private StagingPerson() : base()
    {
        FamilyNameArabic = string.Empty;
        FirstNameArabic = string.Empty;
        FatherNameArabic = string.Empty;
    }

    // ==================== FACTORY METHOD ====================

    /// <summary>
    /// Create a new StagingPerson record from .uhc package data.
    /// </summary>
    public static StagingPerson Create(
        Guid importPackageId,
        Guid originalEntityId,
        string familyNameArabic,
        string firstNameArabic,
        string fatherNameArabic,
        // --- optional: from command ---
        string? motherNameArabic = null,
        string? nationalId = null,
        int? yearOfBirth = null,
        string? email = null,
        string? mobileNumber = null,
        string? phoneNumber = null,
        // --- optional: future expansion ---
        string? fullNameEnglish = null,
        string? gender = null,
        string? nationality = null,
        Guid? originalHouseholdId = null,
        string? relationshipToHead = null)
    {
        var entity = new StagingPerson
        {
            FamilyNameArabic = familyNameArabic,
            FirstNameArabic = firstNameArabic,
            FatherNameArabic = fatherNameArabic,
            MotherNameArabic = motherNameArabic,
            NationalId = nationalId,
            YearOfBirth = yearOfBirth,
            Email = email,
            MobileNumber = mobileNumber,
            PhoneNumber = phoneNumber,
            FullNameEnglish = fullNameEnglish,
            Gender = gender,
            Nationality = nationality,
            OriginalHouseholdId = originalHouseholdId,
            RelationshipToHead = relationshipToHead
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
