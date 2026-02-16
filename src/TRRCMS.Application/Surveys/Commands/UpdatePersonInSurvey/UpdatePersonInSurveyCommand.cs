using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UpdatePersonInSurvey;

/// <summary>
/// Command to update a person in the context of a survey
/// Mirrors AddPersonToHouseholdCommand fields (all nullable for partial update)
/// </summary>
public class UpdatePersonInSurveyCommand : IRequest<PersonDto>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Household ID (from route)
    /// </summary>
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// Person ID to update (from route)
    /// </summary>
    public Guid PersonId { get; set; }

    // ==================== PERSONAL IDENTIFICATION ====================

    /// <summary>
    /// الكنية - Family/Last name in Arabic
    /// </summary>
    public string? FamilyNameArabic { get; set; }

    /// <summary>
    /// الاسم الأول - First name in Arabic
    /// </summary>
    public string? FirstNameArabic { get; set; }

    /// <summary>
    /// اسم الأب - Father's name in Arabic
    /// </summary>
    public string? FatherNameArabic { get; set; }

    /// <summary>
    /// الاسم الأم - Mother's name in Arabic
    /// </summary>
    public string? MotherNameArabic { get; set; }

    /// <summary>
    /// الرقم الوطني - National ID number
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// الجنس - Gender
    /// </summary>
    public int? Gender { get; set; }

    /// <summary>
    /// الجنسية - Nationality
    /// </summary>
    public int? Nationality { get; set; }

    /// <summary>
    /// تاريخ الميلاد - Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    // ==================== CONTACT INFORMATION ====================

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

    // ==================== HOUSEHOLD RELATIONSHIP ====================

    /// <summary>
    /// Relationship to head of household
    /// </summary>
    public int? RelationshipToHead { get; set; }
}
