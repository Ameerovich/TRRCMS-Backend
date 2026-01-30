using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Surveys.Commands.AddPersonToHousehold;

/// <summary>
/// Command to add a person to a household in survey context
/// إضافة شخص جديد - matches mobile/desktop UI
/// </summary>
public class AddPersonToHouseholdCommand : IRequest<PersonDto>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Household ID to add person to
    /// </summary>
    public Guid HouseholdId { get; set; }

    // ==================== PERSONAL IDENTIFICATION (Step 1) ====================

    /// <summary>
    /// الكنية - Family/Last name in Arabic (required)
    /// </summary>
    public string FamilyNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// الاسم الأول - First name in Arabic (required)
    /// </summary>
    public string FirstNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// اسم الأب - Father's name in Arabic (required)
    /// </summary>
    public string FatherNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// الاسم الأم - Mother's name in Arabic (optional)
    /// </summary>
    public string? MotherNameArabic { get; set; }

    /// <summary>
    /// الرقم الوطني - National ID number (optional)
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// تاريخ الميلاد - Year of birth (optional)
    /// </summary>
    public int? YearOfBirth { get; set; }

    // ==================== CONTACT INFORMATION (Step 2) ====================

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

    // ==================== HOUSEHOLD RELATIONSHIP ====================

    /// <summary>
    /// Relationship to head of household (optional)
    /// </summary>
    public string? RelationshipToHead { get; set; }
}
