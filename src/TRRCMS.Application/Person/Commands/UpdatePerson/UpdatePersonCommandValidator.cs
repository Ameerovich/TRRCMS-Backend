using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Persons.Commands.UpdatePerson;

/// <summary>
/// Validator for UpdatePersonCommand.
/// </summary>
public class UpdatePersonCommandValidator : LocalizedValidator<UpdatePersonCommand>
{
    public UpdatePersonCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(L("PersonId_Required"));


        RuleFor(x => x.FamilyNameArabic)
            .MaximumLength(100)
            .WithMessage(L("LastName_MaxLength100"))
            .When(x => !string.IsNullOrEmpty(x.FamilyNameArabic));

        RuleFor(x => x.FirstNameArabic)
            .MaximumLength(100)
            .WithMessage(L("FirstName_MaxLength100"))
            .When(x => !string.IsNullOrEmpty(x.FirstNameArabic));

        RuleFor(x => x.FatherNameArabic)
            .MaximumLength(100)
            .WithMessage(L("FatherName_MaxLength100"))
            .When(x => !string.IsNullOrEmpty(x.FatherNameArabic));

        RuleFor(x => x.MotherNameArabic)
            .MaximumLength(100)
            .WithMessage(L("MotherName_MaxLength100"))
            .When(x => !string.IsNullOrEmpty(x.MotherNameArabic));


        // Syria National ID: exactly 11 digits
        RuleFor(x => x.NationalId)
            .Matches(@"^\d{11}$")
            .When(x => !string.IsNullOrEmpty(x.NationalId))
            .WithMessage(L("NationalId_Exactly11Digits"));

        RuleFor(x => x.DateOfBirth)
            .Must(date => date <= DateTime.UtcNow)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage(L("BirthDate_NotFuture"));


        RuleFor(x => x.Email)
            .MaximumLength(255)
            .WithMessage(L("PersonEmail_MaxLength255"))
            .EmailAddress()
            .WithMessage(L("PersonEmail_InvalidFormat"))
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.MobileNumber)
            .Matches(@"^(\+963|0)9\d{8}$")
            .WithMessage(L("Mobile_SyrianFormat"))
            .When(x => !string.IsNullOrEmpty(x.MobileNumber));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+963|0)\d{7,9}$")
            .WithMessage(L("Landline_SyrianFormat"))
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Gender)
            .Must(v => vocabService.IsValidCode("gender", (int)v!.Value))
            .When(x => x.Gender.HasValue)
            .WithMessage(L("Gender_Invalid"));

        RuleFor(x => x.Nationality)
            .Must(v => vocabService.IsValidCode("nationality", (int)v!.Value))
            .When(x => x.Nationality.HasValue)
            .WithMessage(L("Nationality_Invalid"));
    }
}
