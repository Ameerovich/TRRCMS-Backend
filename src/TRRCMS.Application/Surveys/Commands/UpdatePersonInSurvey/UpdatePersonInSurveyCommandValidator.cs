using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UpdatePersonInSurvey;

/// <summary>
/// Validator for UpdatePersonInSurveyCommand
/// Mirrors AddPersonToHouseholdCommandValidator rules
/// </summary>
public class UpdatePersonInSurveyCommandValidator : AbstractValidator<UpdatePersonInSurveyCommand>
{
    public UpdatePersonInSurveyCommandValidator()
    {
        // ==================== IDs ====================

        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("معرف المسح مطلوب");

        RuleFor(x => x.HouseholdId)
            .NotEmpty()
            .WithMessage("معرف الأسرة مطلوب");

        RuleFor(x => x.PersonId)
            .NotEmpty()
            .WithMessage("معرف الشخص مطلوب");

        // ==================== NAMES ====================

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

        RuleFor(x => x.NationalId)
            .Matches(@"^\d{11}$")
            .When(x => !string.IsNullOrEmpty(x.NationalId))
            .WithMessage("الرقم الوطني يجب أن يكون 11 رقماً بالضبط");

        RuleFor(x => x.DateOfBirth)
            .Must(date => date <= DateTime.UtcNow)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("تاريخ الميلاد لا يمكن أن يكون في المستقبل");

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

        // ==================== HOUSEHOLD RELATIONSHIP ====================

        RuleFor(x => x.RelationshipToHead)
            .IsInEnum()
            .When(x => x.RelationshipToHead.HasValue)
            .WithMessage("علاقة الشخص برب الأسرة غير صالحة");
    }
}
