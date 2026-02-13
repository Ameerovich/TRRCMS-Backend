using MediatR;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Enums;

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
    /// الكنية - Family/Last name in Arabic (optional)
    /// </summary>
    public string? FamilyNameArabic { get; set; }

    /// <summary>
    /// الاسم الأول - First name in Arabic (optional)
    /// </summary>
    public string? FirstNameArabic { get; set; }

    /// <summary>
    /// اسم الأب - Father's name in Arabic (optional)
    /// </summary>
    public string? FatherNameArabic { get; set; }

    /// <summary>
    /// الاسم الأم - Mother's name in Arabic (optional)
    /// </summary>
    public string? MotherNameArabic { get; set; }

    /// <summary>
    /// الرقم الوطني - National ID number (optional)
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// الجنس - Gender (optional)
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// الجنسية - Nationality (optional)
    /// </summary>
    public Nationality? Nationality { get; set; }

    /// <summary>
    /// تاريخ الميلاد - Date of birth (full date or year-only, optional)
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

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
    public RelationshipToHead? RelationshipToHead { get; set; }
}
