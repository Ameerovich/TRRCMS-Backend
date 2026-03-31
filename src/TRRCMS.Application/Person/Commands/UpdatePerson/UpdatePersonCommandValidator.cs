using FluentValidation;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Persons.Commands.UpdatePerson;

/// <summary>
/// Validator for UpdatePersonCommand.
/// </summary>
public class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>
{
    public UpdatePersonCommandValidator(IVocabularyValidationService vocabService)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("معرف الشخص مطلوب");


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


        // Syria National ID: exactly 11 digits
        RuleFor(x => x.NationalId)
            .Matches(@"^\d{11}$")
            .When(x => !string.IsNullOrEmpty(x.NationalId))
            .WithMessage("الرقم الوطني يجب أن يكون 11 رقماً بالضبط");

        RuleFor(x => x.DateOfBirth)
            .Must(date => date <= DateTime.UtcNow)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("تاريخ الميلاد لا يمكن أن يكون في المستقبل");


        RuleFor(x => x.Email)
            .MaximumLength(255)
            .WithMessage("البريد الالكتروني يجب ألا يتجاوز 255 حرف")
            .EmailAddress()
            .WithMessage("صيغة البريد الالكتروني غير صحيحة")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.MobileNumber)
            .Matches(@"^(\+963|0)9\d{8}$")
            .WithMessage("رقم الموبايل يجب أن يكون بالصيغة السورية: 09XXXXXXXX أو 9639XXXXXXXX+")
            .When(x => !string.IsNullOrEmpty(x.MobileNumber));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+963|0)\d{7,9}$")
            .WithMessage("رقم الهاتف يجب أن يكون بالصيغة السورية: 0XXXXXXXXX أو 963XXXXXXXXX+")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Gender)
            .Must(v => vocabService.IsValidCode("gender", (int)v!.Value))
            .When(x => x.Gender.HasValue)
            .WithMessage("الجنس غير صالح");

        RuleFor(x => x.Nationality)
            .Must(v => vocabService.IsValidCode("nationality", (int)v!.Value))
            .When(x => x.Nationality.HasValue)
            .WithMessage("الجنسية غير صالحة");
    }
}
