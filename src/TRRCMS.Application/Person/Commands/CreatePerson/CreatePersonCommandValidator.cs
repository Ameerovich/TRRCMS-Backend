using FluentValidation;

namespace TRRCMS.Application.Persons.Commands.CreatePerson;

/// <summary>
/// Validator for CreatePersonCommand
/// </summary>
public class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>
{
    public CreatePersonCommandValidator()
    {
        // ==================== REQUIRED NAMES ====================

        RuleFor(x => x.FamilyNameArabic)
            .NotEmpty()
            .WithMessage("الكنية مطلوبة")
            .MaximumLength(100)
            .WithMessage("الكنية يجب ألا تتجاوز 100 حرف");

        RuleFor(x => x.FirstNameArabic)
            .NotEmpty()
            .WithMessage("الاسم الأول مطلوب")
            .MaximumLength(100)
            .WithMessage("الاسم الأول يجب ألا يتجاوز 100 حرف");

        RuleFor(x => x.FatherNameArabic)
            .NotEmpty()
            .WithMessage("اسم الأب مطلوب")
            .MaximumLength(100)
            .WithMessage("اسم الأب يجب ألا يتجاوز 100 حرف");

        // ==================== OPTIONAL NAMES ====================

        RuleFor(x => x.MotherNameArabic)
            .MaximumLength(100)
            .WithMessage("الاسم الأم يجب ألا يتجاوز 100 حرف")
            .When(x => !string.IsNullOrEmpty(x.MotherNameArabic));

        // ==================== IDENTIFICATION ====================

        RuleFor(x => x.NationalId)
            .MaximumLength(50)
            .WithMessage("الرقم الوطني يجب ألا يتجاوز 50 حرف")
            .Matches(@"^[0-9]*$")
            .WithMessage("الرقم الوطني يجب أن يحتوي على أرقام فقط")
            .When(x => !string.IsNullOrEmpty(x.NationalId));

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
