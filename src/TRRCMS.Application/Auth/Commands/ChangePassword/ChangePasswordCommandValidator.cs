using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Validator for ChangePasswordCommand.
/// Password rules are read from the currently active SecurityPolicy so admin-configurable
/// values (min length, character-class requirements) actually apply at validation time.
/// </summary>
public class ChangePasswordCommandValidator : LocalizedValidator<ChangePasswordCommand>
{
    private readonly ISecurityPolicyRepository _securityPolicyRepository;

    public ChangePasswordCommandValidator(
        IStringLocalizer<ValidationMessages> localizer,
        ISecurityPolicyRepository securityPolicyRepository) : base(localizer)
    {
        _securityPolicyRepository = securityPolicyRepository;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(L("UserId_Required"));

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(L("CurrentPassword_Required"));

        // Strong password policy — dynamic, sourced from the active SecurityPolicy.
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(L("Password_Required"))
            .CustomAsync(ValidateAgainstActivePasswordPolicyAsync);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(L("PasswordConfirm_Required"))
            .Equal(x => x.NewPassword).WithMessage(L("PasswordConfirm_Mismatch"));

        // New password must be different from current
        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage(L("Password_MustDiffer"));

        RuleFor(x => x.ModifiedByUserId)
            .NotEmpty().WithMessage(L("ModifiedByUserId_Required"));
    }

    private async Task ValidateAgainstActivePasswordPolicyAsync(
        string password,
        FluentValidation.ValidationContext<ChangePasswordCommand> ctx,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(password)) return; // NotEmpty rule already reported this

        var active = await _securityPolicyRepository.GetActiveAsync(cancellationToken);
        var policy = active?.PasswordPolicy ?? PasswordPolicy.Default();

        if (password.Length < policy.MinLength)
            ctx.AddFailure("NewPassword", L("Password_MinLength", policy.MinLength));

        if (policy.RequireUppercase && !Regex.IsMatch(password, "[A-Z]"))
            ctx.AddFailure("NewPassword", L("Password_RequireUpper"));

        if (policy.RequireLowercase && !Regex.IsMatch(password, "[a-z]"))
            ctx.AddFailure("NewPassword", L("Password_RequireLower"));

        if (policy.RequireDigit && !Regex.IsMatch(password, "[0-9]"))
            ctx.AddFailure("NewPassword", L("Password_RequireDigit"));

        if (policy.RequireSpecialCharacter && !Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            ctx.AddFailure("NewPassword", L("Password_RequireSpecial"));
    }
}
