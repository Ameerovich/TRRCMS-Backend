using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.CreateOfficeSurvey;

/// <summary>
/// Validator for CreateOfficeSurveyCommand
/// Office survey creation validation rules
/// </summary>
public class CreateOfficeSurveyCommandValidator : AbstractValidator<CreateOfficeSurveyCommand>
{
    public CreateOfficeSurveyCommandValidator()
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty()
            .WithMessage("Building ID is required");

        RuleFor(x => x.SurveyDate)
            .NotEmpty()
            .WithMessage("Survey date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Survey date cannot be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 2000 characters");

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
