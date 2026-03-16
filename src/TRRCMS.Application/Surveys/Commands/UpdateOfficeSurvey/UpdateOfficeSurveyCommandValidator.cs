using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.UpdateOfficeSurvey;

/// <summary>
/// Validator for UpdateOfficeSurveyCommand
/// Office survey update validation rules
/// </summary>
public class UpdateOfficeSurveyCommandValidator : AbstractValidator<UpdateOfficeSurveyCommand>
{
    public UpdateOfficeSurveyCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.SurveyDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.SurveyDate.HasValue)
            .WithMessage("Survey date cannot be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be greater than 0 minutes")
            .LessThanOrEqualTo(1440) // Max 24 hours
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration cannot exceed 1440 minutes (24 hours)");

        RuleFor(x => x.OfficeLocation)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.OfficeLocation))
            .WithMessage("Office location cannot exceed 200 characters");

        RuleFor(x => x.RegistrationNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.RegistrationNumber))
            .WithMessage("Registration number cannot exceed 50 characters");

        RuleFor(x => x.AppointmentReference)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.AppointmentReference))
            .WithMessage("Appointment reference cannot exceed 50 characters");

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone))
            .WithMessage("Contact phone cannot exceed 20 characters")
            .Matches(@"^[\d\+\-\s\(\)]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone))
            .WithMessage("Contact phone contains invalid characters");

        RuleFor(x => x.ContactEmail)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail))
            .WithMessage("Contact email cannot exceed 100 characters")
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail))
            .WithMessage("Contact email is not a valid email address");
    }
}
