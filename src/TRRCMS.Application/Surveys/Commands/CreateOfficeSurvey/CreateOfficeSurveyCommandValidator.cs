using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.CreateOfficeSurvey;

/// <summary>
/// Validator for CreateOfficeSurveyCommand
/// Office survey creation validation rules
/// </summary>
public class CreateOfficeSurveyCommandValidator : LocalizedValidator<CreateOfficeSurveyCommand>
{
    public CreateOfficeSurveyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage(L("BuildingId_Required"));

        RuleFor(x => x.SurveyDate)
            .NotEmpty()
            .WithMessage(L("SurveyDate_Required"))
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage(L("SurveyDate_NotFuture"));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage(L("Notes_MaxLength2000"));

        RuleFor(x => x.OfficeLocation)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.OfficeLocation))
            .WithMessage(L("OfficeLocation_MaxLength200"));

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.RegistrationNumber))
            .WithMessage(L("RegistrationNumber_MaxLength50"));

        RuleFor(x => x.AppointmentReference)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.AppointmentReference))
            .WithMessage(L("AppointmentRef_MaxLength50"));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone))
            .WithMessage(L("ContactPhone_MaxLength20"))
            .Matches(@"^[\d\+\-\s\(\)]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone))
            .WithMessage(L("ContactPhone_InvalidChars"));

        RuleFor(x => x.ContactEmail)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail))
            .WithMessage(L("ContactEmail_MaxLength100"))
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail))
            .WithMessage(L("ContactEmail_Invalid"));
    }
}
