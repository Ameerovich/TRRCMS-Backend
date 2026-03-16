using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.CreateContactPerson;

/// <summary>
/// Validator for CreateContactPersonCommand
/// </summary>
public class CreateContactPersonCommandValidator : AbstractValidator<CreateContactPersonCommand>
{
    public CreateContactPersonCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("معرف المسح مطلوب");

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

        RuleFor(x => x.FamilyNameArabic)
            .NotEmpty()
            .WithMessage("الكنية مطلوبة")
            .MaximumLength(100)
            .WithMessage("الكنية يجب ألا تتجاوز 100 حرف");

        RuleFor(x => x.MotherNameArabic)
            .NotEmpty()
            .WithMessage("اسم الأم مطلوب")
            .MaximumLength(100)
            .WithMessage("اسم الأم يجب ألا يتجاوز 100 حرف");

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
