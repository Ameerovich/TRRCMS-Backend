using System.Text.Json.Serialization;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Persons.Dtos;

/// <summary>
/// Simplified Person DTO for frontend form
/// إضافة شخص جديد - matches mobile/desktop UI
/// </summary>
public class PersonDto
{
    // ==================== IDENTIFIER ====================

    /// <summary>
    /// Person ID
    /// </summary>
    public Guid Id { get; set; }

    // ==================== PERSONAL IDENTIFICATION (Step 1) ====================

    /// <summary>
    /// الكنية - Family/Last name in Arabic
    /// </summary>
    public string FamilyNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// الاسم الأول - First name in Arabic
    /// </summary>
    public string FirstNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// اسم الأب - Father's name in Arabic
    /// </summary>
    public string FatherNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// الاسم الأم - Mother's name in Arabic
    /// </summary>
    public string? MotherNameArabic { get; set; }

    /// <summary>
    /// الرقم الوطني - National ID number
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// الجنس - Gender (returned as string: "Male", "Female")
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Gender? Gender { get; set; }

    /// <summary>
    /// الجنسية - Nationality (returned as string: "Syrian", "Palestinian", etc.)
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Nationality? Nationality { get; set; }

    /// <summary>
    /// تاريخ الميلاد - Date of birth (full date or year-only)
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    // ==================== CONTACT INFORMATION (Step 2) ====================

    /// <summary>
    /// البريد الالكتروني - Email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// رقم الموبايل - Mobile phone number
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// رقم الهاتف - Landline phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    // ==================== HOUSEHOLD CONTEXT ====================

    /// <summary>
    /// Household ID (if person belongs to a household)
    /// </summary>
    public Guid? HouseholdId { get; set; }

    /// <summary>
    /// Relationship to head of household (علاقة برب الأسرة) - returned as string: "Head", "Spouse", "Son", etc.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RelationshipToHead? RelationshipToHead { get; set; }

    // ==================== AUDIT FIELDS ====================

    /// <summary>
    /// Creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// User who created this record
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Last modification timestamp (UTC)
    /// </summary>
    public DateTime? LastModifiedAtUtc { get; set; }

    /// <summary>
    /// User who last modified this record
    /// </summary>
    public Guid? LastModifiedBy { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Deletion timestamp (UTC)
    /// </summary>
    public DateTime? DeletedAtUtc { get; set; }

    /// <summary>
    /// User who deleted this record
    /// </summary>
    public Guid? DeletedBy { get; set; }

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// Full name in Arabic (computed: الاسم الأول + اسم الأب + الكنية)
    /// </summary>
    public string FullNameArabic => $"{FirstNameArabic} {FatherNameArabic} {FamilyNameArabic}".Trim();

    /// <summary>
    /// Calculated age based on date of birth
    /// </summary>
    public int? Age
    {
        get
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
}