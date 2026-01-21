using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Surveys.Commands.AddPersonToHousehold;

/// <summary>
/// Command to add a person/member to a household in survey context
/// Corresponds to UC-001 Stage 3: Person Registration
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

    // ==================== PERSONAL IDENTIFICATION (ARABIC NAMES - REQUIRED) ====================

    /// <summary>
    /// First name in Arabic (الاسم الأول) - Required
    /// </summary>
    public string FirstNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// Father's name in Arabic (اسم الأب) - Required
    /// </summary>
    public string FatherNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// Family/Last name in Arabic (اسم العائلة) - Required
    /// </summary>
    public string FamilyNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// Mother's name in Arabic (اسم الأم) - Optional
    /// </summary>
    public string? MotherNameArabic { get; set; }

    /// <summary>
    /// Full name in English (optional)
    /// </summary>
    public string? FullNameEnglish { get; set; }

    // ==================== IDENTIFICATION DOCUMENTS ====================

    /// <summary>
    /// National ID or identification number
    /// </summary>
    public string? NationalId { get; set; }

    // ==================== DEMOGRAPHICS ====================

    /// <summary>
    /// Year of birth (integer, not full date)
    /// </summary>
    public int? YearOfBirth { get; set; }

    /// <summary>
    /// Gender (M/F)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Nationality
    /// </summary>
    public string? Nationality { get; set; }

    // ==================== HOUSEHOLD RELATIONSHIP ====================

    /// <summary>
    /// Relationship to head of household
    /// Examples: "Head", "Spouse", "Son", "Daughter", "Parent", etc.
    /// Arabic examples: "رب الأسرة", "الزوج/الزوجة", "الابن", "الابنة"
    /// </summary>
    public string? RelationshipToHead { get; set; }

    // ==================== CONTACT INFORMATION ====================

    /// <summary>
    /// Primary phone number
    /// </summary>
    public string? PrimaryPhoneNumber { get; set; }

    /// <summary>
    /// Secondary phone number
    /// </summary>
    public string? SecondaryPhoneNumber { get; set; }

    /// <summary>
    /// Is this person the main contact person?
    /// </summary>
    public bool? IsContactPerson { get; set; }
}