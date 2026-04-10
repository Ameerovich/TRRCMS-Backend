using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Application.SecuritySettings.Commands.UpdateSecuritySettings;

/// <summary>
/// Validator for UpdateSecuritySettingsCommand.
/// Prevents configurations that violate hard safety constraints or would make the system unusable.
/// </summary>
public class UpdateSecuritySettingsCommandValidator : LocalizedValidator<UpdateSecuritySettingsCommand>
{
    public UpdateSecuritySettingsCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        // Password policy validation

        RuleFor(x => x.PasswordMinLength)
            .InclusiveBetween(PasswordPolicy.AbsoluteMinLength, PasswordPolicy.AbsoluteMaxLength)
            .WithMessage(L("PasswordPolicy_MinLength", PasswordPolicy.AbsoluteMinLength, PasswordPolicy.AbsoluteMaxLength));

        RuleFor(x => x.PasswordExpiryDays)
            .InclusiveBetween(0, PasswordPolicy.MaxExpiryDays)
            .WithMessage(L("PasswordPolicy_ExpiryDays", PasswordPolicy.MaxExpiryDays));

        RuleFor(x => x.PasswordReuseHistory)
            .InclusiveBetween(0, PasswordPolicy.MaxReuseHistory)
            .WithMessage(L("PasswordPolicy_ReuseHistory", PasswordPolicy.MaxReuseHistory));

        // Cross-field: if no complexity requirements, enforce higher min length
        RuleFor(x => x.PasswordMinLength)
            .GreaterThanOrEqualTo(12)
            .When(x => !x.PasswordRequireUppercase
                    && !x.PasswordRequireLowercase
                    && !x.PasswordRequireDigit
                    && !x.PasswordRequireSpecialCharacter)
            .WithMessage(L("PasswordPolicy_MinLengthNoComplexity"));

        // Session and lockout validation

        RuleFor(x => x.SessionTimeoutMinutes)
            .InclusiveBetween(SessionLockoutPolicy.MinSessionTimeout, SessionLockoutPolicy.MaxSessionTimeout)
            .WithMessage(L("Session_Timeout", SessionLockoutPolicy.MinSessionTimeout, SessionLockoutPolicy.MaxSessionTimeout));

        RuleFor(x => x.MaxFailedLoginAttempts)
            .InclusiveBetween(SessionLockoutPolicy.MinFailedAttempts, SessionLockoutPolicy.MaxFailedAttempts)
            .WithMessage(L("Session_MaxFailedAttempts", SessionLockoutPolicy.MinFailedAttempts, SessionLockoutPolicy.MaxFailedAttempts));

        RuleFor(x => x.LockoutDurationMinutes)
            .InclusiveBetween(SessionLockoutPolicy.MinLockoutDuration, SessionLockoutPolicy.MaxLockoutDuration)
            .WithMessage(L("Session_LockoutDuration", SessionLockoutPolicy.MinLockoutDuration, SessionLockoutPolicy.MaxLockoutDuration));

        // Access control validation

        // At least one authentication method must remain enabled (S06 safety constraint)
        RuleFor(x => x)
            .Must(x => x.AllowPasswordAuthentication || x.AllowSsoAuthentication || x.AllowTokenAuthentication)
            .WithMessage(L("Auth_AtLeastOneMethod"));

        // IP allowlist must not be empty when enforced
        RuleFor(x => x.IpAllowlist)
            .NotEmpty()
            .When(x => x.EnforceIpAllowlist)
            .WithMessage(L("IpAllowlist_NotEmpty"));

        // Validate IP allowlist format when provided
        RuleFor(x => x.IpAllowlist)
            .Must(BeValidIpList!)
            .When(x => !string.IsNullOrWhiteSpace(x.IpAllowlist))
            .WithMessage(L("IpAllowlist_InvalidEntries"));

        // Validate IP denylist format when provided
        RuleFor(x => x.IpDenylist)
            .Must(BeValidIpList!)
            .When(x => !string.IsNullOrWhiteSpace(x.IpDenylist))
            .WithMessage(L("IpDenylist_InvalidEntries"));

        // Allowed environments must not be empty when restriction is enabled
        RuleFor(x => x.AllowedEnvironments)
            .NotEmpty()
            .When(x => x.RestrictByEnvironment)
            .WithMessage(L("AllowedEnv_NotEmpty"));

        // Metadata

        RuleFor(x => x.ChangeDescription)
            .MaximumLength(1000)
            .WithMessage(L("ChangeDescription_MaxLength1000"));
    }

    /// <summary>
    /// Validates that a comma-separated string contains valid IPv4, IPv6, or CIDR entries.
    /// </summary>
    private static bool BeValidIpList(string ipList)
    {
        if (string.IsNullOrWhiteSpace(ipList))
            return true;

        var entries = ipList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var entry in entries)
        {
            // Handle CIDR notation (e.g., "192.168.1.0/24")
            var parts = entry.Split('/');
            if (parts.Length > 2) return false;

            if (!System.Net.IPAddress.TryParse(parts[0], out _))
                return false;

            if (parts.Length == 2 && (!int.TryParse(parts[1], out var prefix) || prefix < 0 || prefix > 128))
                return false;
        }

        return true;
    }
}
