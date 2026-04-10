using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.CreateContactPerson;

/// <summary>
/// Validator for CreateContactPersonCommand
/// </summary>
public class CreateContactPersonCommandValidator : LocalizedValidator<CreateContactPersonCommand>
{
    public CreateContactPersonCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.FirstNameArabic)
            .NotEmpty()
            .WithMessage(L("FirstName_Required"))
            .MaximumLength(100)
            .WithMessage(L("FirstName_MaxLength100"));

        RuleFor(x => x.FatherNameArabic)
            .NotEmpty()
            .WithMessage(L("FatherName_Required"))
            .MaximumLength(100)
            .WithMessage(L("FatherName_MaxLength100"));

        RuleFor(x => x.FamilyNameArabic)
            .NotEmpty()
            .WithMessage(L("LastName_Required"))
            .MaximumLength(100)
            .WithMessage(L("LastName_MaxLength100"));

        RuleFor(x => x.MotherNameArabic)
            .NotEmpty()
            .WithMessage(L("MotherName_Required"))
            .MaximumLength(100)
            .WithMessage(L("MotherName_MaxLength100"));

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
    }
}
