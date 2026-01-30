using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Persons.Commands.UpdatePerson;

/// <summary>
/// Command to update an existing person
/// تعديل بيانات شخص - matches mobile/desktop UI
/// </summary>
public class UpdatePersonCommand : IRequest<PersonDto>
{
    /// <summary>
    /// Person ID to update
    /// </summary>
    public Guid Id { get; set; }

    // ==================== PERSONAL IDENTIFICATION (Step 1) ====================

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
    /// تاريخ الميلاد - Year of birth
    /// </summary>
    public int? YearOfBirth { get; set; }

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
}
