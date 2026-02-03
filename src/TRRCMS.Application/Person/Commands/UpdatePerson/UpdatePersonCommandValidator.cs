using FluentValidation;

namespace TRRCMS.Application.Persons.Commands.UpdatePerson;

/// <summary>
/// Validator for UpdatePersonCommand
/// Enhanced with Syria-specific National ID validation (11 digits per FSD)
/// </summary>
public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
{
    public UpdatePersonCommandValidator()
    {
        // ==================== ID ====================

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("معرف الشخص مطلوب");

        // ==================== NAMES (if provided) ====================

        RuleFor(x => x.FamilyNameArabic)
            .MaximumLength(100)
            .WithMessage("الكنية يجب ألا تتجاوز 100 حرف")
            .When(x => !string.IsNullOrEmpty(x.FamilyNameArabic));

        RuleFor(x => x.FirstNameArabic)
            .MaximumLength(100)
            .WithMessage("الاسم الأول يجب ألا يتجاوز 100 حرف")
            .When(x => !string.IsNullOrEmpty(x.FirstNameArabic));

        RuleFor(x => x.FatherNameArabic)
            .MaximumLength(100)
            .WithMessage("اسم الأب يجب ألا يتجاوز 100 حرف")
            .When(x => !string.IsNullOrEmpty(x.FatherNameArabic));

        RuleFor(x => x.MotherNameArabic)
            .MaximumLength(100)
            .WithMessage("الاسم الأم يجب ألا يتجاوز 100 حرف")
            .When(x => !string.IsNullOrEmpty(x.MotherNameArabic));

        // ==================== IDENTIFICATION ====================

        // Syria National ID: exactly 11 digits (per FSD)
        RuleFor(x => x.NationalId)
            .Matches(@"^\d{11}$")
            .When(x => !string.IsNullOrEmpty(x.NationalId))
            .WithMessage("الرقم الوطني يجب أن يكون 11 رقماً بالضبط");

        RuleFor(x => x.YearOfBirth)
            .InclusiveBetween(1900, DateTime.UtcNow.Year)
            .WithMessage($"سنة الميلاد يجب أن تكون بين 1900 و {DateTime.UtcNow.Year}")
            .When(x => x.YearOfBirth.HasValue);

        // ==================== CONTACT INFORMATION ====================

        RuleFor(x => x.Email)
            .MaximumLength(255)
            .WithMessage("البريد الالكتروني يجب ألا يتجاوز 255 حرف")
            .EmailAddress()
            .WithMessage("صيغة البريد الالكتروني غير صحيحة")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.MobileNumber)
            .MaximumLength(20)
            .WithMessage("رقم الموبايل يجب ألا يتجاوز 20 رقم")
            .Matches(@"^[\+]?[0-9\s\-]*$")
            .WithMessage("رقم الموبايل غير صحيح")
            .When(x => !string.IsNullOrEmpty(x.MobileNumber));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithMessage("رقم الهاتف يجب ألا يتجاوز 20 رقم")
            .Matches(@"^[\+]?[0-9\s\-]*$")
            .WithMessage("رقم الهاتف غير صحيح")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
