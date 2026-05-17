using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;
using TRRCMS.Domain.Enums;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Application.Users.Commands.CreateUser;

/// <summary>
/// Validator for CreateUserCommand
/// Enforces strong password policy and data integrity.
/// Password rules are read from the currently active SecurityPolicy so the
/// admin-configurable values (min length, character-class requirements) actually
/// apply at validation time instead of being hardcoded.
/// </summary>
public class CreateUserCommandValidator : LocalizedValidator<CreateUserCommand>
{
    private readonly ISecurityPolicyRepository _securityPolicyRepository;

    public CreateUserCommandValidator(
        IStringLocalizer<ValidationMessages> localizer,
        ISecurityPolicyRepository securityPolicyRepository) : base(localizer)
    {
        _securityPolicyRepository = securityPolicyRepository;
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

        // Strong password policy — dynamic, sourced from the active SecurityPolicy.
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(L("Password_Required"))
            .CustomAsync(ValidateAgainstActivePasswordPolicyAsync);

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

    private async Task ValidateAgainstActivePasswordPolicyAsync(
        string password,
        FluentValidation.ValidationContext<CreateUserCommand> ctx,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(password)) return; // NotEmpty rule already reported this

        var active = await _securityPolicyRepository.GetActiveAsync(cancellationToken);
        var policy = active?.PasswordPolicy ?? PasswordPolicy.Default();

        if (password.Length < policy.MinLength)
            ctx.AddFailure("Password", L("Password_MinLength", policy.MinLength));

        if (policy.RequireUppercase && !Regex.IsMatch(password, "[A-Z]"))
            ctx.AddFailure("Password", L("Password_RequireUpper"));

        if (policy.RequireLowercase && !Regex.IsMatch(password, "[a-z]"))
            ctx.AddFailure("Password", L("Password_RequireLower"));

        if (policy.RequireDigit && !Regex.IsMatch(password, "[0-9]"))
            ctx.AddFailure("Password", L("Password_RequireDigit"));

        if (policy.RequireSpecialCharacter && !Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            ctx.AddFailure("Password", L("Password_RequireSpecial"));
    }
}
