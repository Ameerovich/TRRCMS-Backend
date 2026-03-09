using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Surveys.Commands.CreateContactPerson;

/// <summary>
/// Command to add a contact person to a survey (without requiring a household)
/// إضافة شخص الاتصال - يمكن إضافته فور إنشاء المسح
/// </summary>
public class CreateContactPersonCommand : IRequest<PersonDto>
{
    /// <summary>
    /// Survey ID to link the contact person to
    /// </summary>
    public Guid SurveyId { get; set; }

    // ==================== PERSONAL IDENTIFICATION (REQUIRED) ====================

    /// <summary>
    /// الاسم الأول - First name in Arabic (required)
    /// </summary>
    public string FirstNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// اسم الأب - Father's name in Arabic (required)
    /// </summary>
    public string FatherNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// الكنية - Family/Last name in Arabic (required)
    /// </summary>
    public string FamilyNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// اسم الأم - Mother's name in Arabic (required)
    /// </summary>
    public string MotherNameArabic { get; set; } = string.Empty;

    // ==================== OPTIONAL FIELDS ====================

    /// <summary>
    /// الرقم الوطني - National ID number (optional)
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// الجنس - Gender (optional)
    /// </summary>
    public int? Gender { get; set; }

    /// <summary>
    /// الجنسية - Nationality (optional)
    /// </summary>
    public int? Nationality { get; set; }

    /// <summary>
    /// تاريخ الميلاد - Date of birth (optional)
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// البريد الالكتروني - Email address (optional)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// رقم الموبايل - Mobile phone number (optional)
    /// </summary>
    public string? MobileNumber { get; set; }

    /// <summary>
    /// رقم الهاتف - Landline phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }
}
