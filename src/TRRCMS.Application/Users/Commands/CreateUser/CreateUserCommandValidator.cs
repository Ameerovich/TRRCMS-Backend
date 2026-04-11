using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.CreateUser;

/// <summary>
/// Validator for CreateUserCommand
/// Enforces strong password policy and data integrity
/// </summary>
public class CreateUserCommandValidator : LocalizedValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(L("Username_Required"))
            .Length(3, 50).WithMessage(L("Username_Length3to50"))
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage(L("Username_InvalidChars"));

        RuleFor(x => x.FullNameArabic)
            .NotEmpty().WithMessage(L("FullNameAr_Required"))
            .MaximumLength(200).WithMessage(L("FullNameAr_MaxLength200"));

        RuleFor(x => x.FullNameEnglish)
            .MaximumLength(200).WithMessage(L("FullNameEn_MaxLength200"))
            .When(x => !string.IsNullOrWhiteSpace(x.FullNameEnglish));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(L("Email_Required"))
            .EmailAddress().WithMessage(L("Email_InvalidFormat"))
            .MaximumLength(100).WithMessage(L("Email_MaxLength100"));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+963|0)\d{7,9}$")
            .WithMessage(L("Phone_SyrianFormat"))
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage(L("UserRole_Invalid"));

        // Strong password policy
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(L("Password_Required"))
            .MinimumLength(8).WithMessage(L("Password_MinLength8"))
            .Matches(@"[A-Z]").WithMessage(L("Password_RequireUpper"))
            .Matches(@"[a-z]").WithMessage(L("Password_RequireLower"))
            .Matches(@"[0-9]").WithMessage(L("Password_RequireDigit"))
            .Matches(@"[^a-zA-Z0-9]").WithMessage(L("Password_RequireSpecial"));

        RuleFor(x => x.Organization)
            .MaximumLength(100).WithMessage(L("Organization_MaxLength100"))
            .When(x => !string.IsNullOrWhiteSpace(x.Organization));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage(L("JobTitle_MaxLength100"))
            .When(x => !string.IsNullOrWhiteSpace(x.JobTitle));

        RuleFor(x => x.EmployeeId)
            .MaximumLength(50).WithMessage(L("EmployeeId_MaxLength50"))
            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeId));

        // Business rule: At least one access type must be selected
        RuleFor(x => x)
            .Must(x => x.HasMobileAccess || x.HasDesktopAccess)
            .WithMessage(L("User_RequireAccess"));
    }
}
