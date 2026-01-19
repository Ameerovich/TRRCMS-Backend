using FluentValidation;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.UpdateUser;

/// <summary>
/// Validator for UpdateUserCommand
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FullNameArabic)
            .MaximumLength(200).WithMessage("Full name in Arabic cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FullNameArabic));

        RuleFor(x => x.FullNameEnglish)
            .MaximumLength(200).WithMessage("Full name in English cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FullNameEnglish));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Invalid phone number format")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role")
            .When(x => x.Role.HasValue);

        RuleFor(x => x.Organization)
            .MaximumLength(100).WithMessage("Organization cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Organization));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("Job title cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.JobTitle));

        RuleFor(x => x.EmployeeId)
            .MaximumLength(50).WithMessage("Employee ID cannot exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeId));
    }
}