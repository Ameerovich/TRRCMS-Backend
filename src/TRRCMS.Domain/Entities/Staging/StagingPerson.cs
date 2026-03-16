using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for Person records from .uhc packages.
/// Mirrors the <see cref="Person"/> production entity in an isolated staging area.
/// Central to duplicate detection (person matching):
/// - NationalId exact match
/// - Arabic name Levenshtein similarity
/// - Phone number match
/// - Year of birth + gender composite
///</summary>
public class StagingPerson : BaseStagingEntity
{
    /// <summary>Family name in Arabic (اسم العائلة).</summary>
    public string FamilyNameArabic { get; private set; }

    /// <summary>First name in Arabic (الاسم الأول).</summary>
    public string FirstNameArabic { get; private set; }

    /// <summary>Father's name in Arabic (اسم الأب).</summary>
    public string FatherNameArabic { get; private set; }

    /// <summary>Mother's name in Arabic (اسم الأم).</summary>
    public string? MotherNameArabic { get; private set; }
    /// <summary>
    /// National ID number — primary key for duplicate detection.
    /// Exact match on this field produces a high-confidence duplicate.
    /// </summary>
    public string? NationalId { get; private set; }

    /// <summary>Date of birth (تاريخ الميلاد) — from command, optional. Used in duplicate detection composite.</summary>
    public DateTime? DateOfBirth { get; private set; }
    /// <summary>Email address.</summary>
    public string? Email { get; private set; }

    /// <summary>Mobile phone number.</summary>
    public string? MobileNumber { get; private set; }

    /// <summary>Landline phone number.</summary>
    public string? PhoneNumber { get; private set; }
    /// <summary>Gender (الجنس).</summary>
    public Gender? Gender { get; private set; }

    /// <summary>Nationality (الجنسية).</summary>
    public Nationality? Nationality { get; private set; }
    /// <summary>
    /// Original Household UUID from .uhc — not a FK to production Households.
    /// Used for intra-batch household structure validation.
    /// </summary>
    public Guid? OriginalHouseholdId { get; private set; }

    /// <summary>Relationship to the head of household (صلة القرابة برب الأسرة).</summary>
    public RelationshipToHead? RelationshipToHead { get; private set; }

    /// <summary>Whether this person is the contact person for the survey.</summary>
    public bool IsContactPerson { get; private set; }
    /// <summary>EF Core constructor.</summary>
    private StagingPerson() : base()
    {
        FamilyNameArabic = string.Empty;
        FirstNameArabic = string.Empty;
        FatherNameArabic = string.Empty;
    }
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
        DateTime? dateOfBirth = null,
        string? email = null,
        string? mobileNumber = null,
        string? phoneNumber = null,
        Gender? gender = null,
        Nationality? nationality = null,
        Guid? originalHouseholdId = null,
        RelationshipToHead? relationshipToHead = null,
        bool isContactPerson = false)
    {
        var entity = new StagingPerson
        {
            FamilyNameArabic = familyNameArabic,
            FirstNameArabic = firstNameArabic,
            FatherNameArabic = fatherNameArabic,
            MotherNameArabic = motherNameArabic,
            NationalId = nationalId,
            DateOfBirth = dateOfBirth,
            Email = email,
            MobileNumber = mobileNumber,
            PhoneNumber = phoneNumber,
            Gender = gender,
            Nationality = nationality,
            OriginalHouseholdId = originalHouseholdId,
            RelationshipToHead = relationshipToHead,
            IsContactPerson = isContactPerson
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
