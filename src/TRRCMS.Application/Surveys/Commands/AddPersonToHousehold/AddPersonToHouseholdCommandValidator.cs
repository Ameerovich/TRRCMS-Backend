using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.AddPersonToHousehold;

/// <summary>
/// Validator for AddPersonToHouseholdCommand
/// </summary>
public class AddPersonToHouseholdCommandValidator : AbstractValidator<AddPersonToHouseholdCommand>
{
    public AddPersonToHouseholdCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.HouseholdId)
            .NotEmpty()
            .WithMessage("Household ID is required");

        // Arabic names - Required
        RuleFor(x => x.FirstNameArabic)
            .NotEmpty()
            .WithMessage("First name in Arabic is required")
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.FatherNameArabic)
            .NotEmpty()
            .WithMessage("Father's name in Arabic is required")
            .MaximumLength(100)
            .WithMessage("Father's name cannot exceed 100 characters");

        RuleFor(x => x.FamilyNameArabic)
            .NotEmpty()
            .WithMessage("Family name in Arabic is required")
            .MaximumLength(100)
            .WithMessage("Family name cannot exceed 100 characters");

        // Mother's name - Optional
        RuleFor(x => x.MotherNameArabic)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.MotherNameArabic))
            .WithMessage("Mother's name cannot exceed 100 characters");

        // English name - Optional
        RuleFor(x => x.FullNameEnglish)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.FullNameEnglish))
            .WithMessage("English name cannot exceed 300 characters");

        // National ID - Optional
        RuleFor(x => x.NationalId)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.NationalId))
            .WithMessage("National ID cannot exceed 50 characters");

        // Year of birth validation
        RuleFor(x => x.YearOfBirth)
            .GreaterThanOrEqualTo(1900)
            .When(x => x.YearOfBirth.HasValue)
            .WithMessage("Year of birth must be 1900 or later")
            .LessThanOrEqualTo(DateTime.UtcNow.Year)
            .When(x => x.YearOfBirth.HasValue)
            .WithMessage("Year of birth cannot be in the future");

        // Gender validation
        RuleFor(x => x.Gender)
            .Must(g => g == null || g == "M" || g == "F" || g == "Male" || g == "Female" ||
                       g == "ذكر" || g == "أنثى")
            .When(x => !string.IsNullOrWhiteSpace(x.Gender))
            .WithMessage("Gender must be M, F, Male, Female, ذكر, or أنثى");

        // Nationality validation
        RuleFor(x => x.Nationality)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Nationality))
            .WithMessage("Nationality cannot exceed 100 characters");

        // Relationship to head
        RuleFor(x => x.RelationshipToHead)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.RelationshipToHead))
            .WithMessage("Relationship to head cannot exceed 100 characters");

        // Phone numbers validation
        RuleFor(x => x.PrimaryPhoneNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.PrimaryPhoneNumber))
            .WithMessage("Primary phone number cannot exceed 20 characters");

        RuleFor(x => x.SecondaryPhoneNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.SecondaryPhoneNumber))
            .WithMessage("Secondary phone number cannot exceed 20 characters");
    }
}