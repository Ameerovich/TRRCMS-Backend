using FluentValidation;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.CreateUser;

/// <summary>
/// Validator for CreateUserCommand
/// Enforces strong password policy and data integrity
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.FullNameArabic)
            .NotEmpty().WithMessage("Full name in Arabic is required")
            .MaximumLength(200).WithMessage("Full name in Arabic cannot exceed 200 characters");

        RuleFor(x => x.FullNameEnglish)
            .MaximumLength(200).WithMessage("Full name in English cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FullNameEnglish));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Invalid phone number format")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role");

        // Strong password policy (FSD Section 11: Security Settings)
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.Organization)
            .MaximumLength(100).WithMessage("Organization cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Organization));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("Job title cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.JobTitle));

        RuleFor(x => x.EmployeeId)
            .MaximumLength(50).WithMessage("Employee ID cannot exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeId));

        // Business rule: At least one access type must be selected
        RuleFor(x => x)
            .Must(x => x.HasMobileAccess || x.HasDesktopAccess)
            .WithMessage("User must have at least mobile or desktop access");
    }
}