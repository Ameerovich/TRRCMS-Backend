using FluentValidation;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Application.SecuritySettings.Commands.UpdateSecuritySettings;

/// <summary>
/// Validator for UpdateSecuritySettingsCommand.
/// Implements UC-011 S06: Validate Security Policy Configuration.
/// Prevents configurations that violate hard safety constraints or would make the system unusable.
/// </summary>
public class UpdateSecuritySettingsCommandValidator : AbstractValidator<UpdateSecuritySettingsCommand>
{
    public UpdateSecuritySettingsCommandValidator()
    {
        // ==================== PASSWORD POLICY VALIDATION (UC-011 S03) ====================

        RuleFor(x => x.PasswordMinLength)
            .InclusiveBetween(PasswordPolicy.AbsoluteMinLength, PasswordPolicy.AbsoluteMaxLength)
            .WithMessage($"Password minimum length must be between {PasswordPolicy.AbsoluteMinLength} and {PasswordPolicy.AbsoluteMaxLength}.");

        RuleFor(x => x.PasswordExpiryDays)
            .InclusiveBetween(0, PasswordPolicy.MaxExpiryDays)
            .WithMessage($"Password expiry must be between 0 (disabled) and {PasswordPolicy.MaxExpiryDays} days.");

        RuleFor(x => x.PasswordReuseHistory)
            .InclusiveBetween(0, PasswordPolicy.MaxReuseHistory)
            .WithMessage($"Password reuse history must be between 0 (disabled) and {PasswordPolicy.MaxReuseHistory}.");

        // Cross-field: if no complexity requirements, enforce higher min length
        RuleFor(x => x.PasswordMinLength)
            .GreaterThanOrEqualTo(12)
            .When(x => !x.PasswordRequireUppercase
                    && !x.PasswordRequireLowercase
                    && !x.PasswordRequireDigit
                    && !x.PasswordRequireSpecialCharacter)
            .WithMessage("When no complexity requirements are set, minimum password length must be at least 12 characters.");

        // ==================== SESSION & LOCKOUT VALIDATION (UC-011 S04) ====================

        RuleFor(x => x.SessionTimeoutMinutes)
            .InclusiveBetween(SessionLockoutPolicy.MinSessionTimeout, SessionLockoutPolicy.MaxSessionTimeout)
            .WithMessage($"Session timeout must be between {SessionLockoutPolicy.MinSessionTimeout} and {SessionLockoutPolicy.MaxSessionTimeout} minutes.");

        RuleFor(x => x.MaxFailedLoginAttempts)
            .InclusiveBetween(SessionLockoutPolicy.MinFailedAttempts, SessionLockoutPolicy.MaxFailedAttempts)
            .WithMessage($"Maximum failed login attempts must be between {SessionLockoutPolicy.MinFailedAttempts} and {SessionLockoutPolicy.MaxFailedAttempts}.");

        RuleFor(x => x.LockoutDurationMinutes)
            .InclusiveBetween(SessionLockoutPolicy.MinLockoutDuration, SessionLockoutPolicy.MaxLockoutDuration)
            .WithMessage($"Lockout duration must be between {SessionLockoutPolicy.MinLockoutDuration} and {SessionLockoutPolicy.MaxLockoutDuration} minutes.");

        // ==================== ACCESS CONTROL VALIDATION (UC-011 S05) ====================

        // At least one authentication method must remain enabled (S06 safety constraint)
        RuleFor(x => x)
            .Must(x => x.AllowPasswordAuthentication || x.AllowSsoAuthentication || x.AllowTokenAuthentication)
            .WithMessage("At least one authentication method must be enabled. Disabling all methods would lock out all users.");

        // IP allowlist must not be empty when enforced
        RuleFor(x => x.IpAllowlist)
            .NotEmpty()
            .When(x => x.EnforceIpAllowlist)
            .WithMessage("IP allowlist cannot be empty when enforcement is enabled. This would block all admin access.");

        // Validate IP allowlist format when provided
        RuleFor(x => x.IpAllowlist)
            .Must(BeValidIpList!)
            .When(x => !string.IsNullOrWhiteSpace(x.IpAllowlist))
            .WithMessage("IP allowlist contains invalid IP addresses or CIDR notation.");

        // Validate IP denylist format when provided
        RuleFor(x => x.IpDenylist)
            .Must(BeValidIpList!)
            .When(x => !string.IsNullOrWhiteSpace(x.IpDenylist))
            .WithMessage("IP denylist contains invalid IP addresses or CIDR notation.");

        // Allowed environments must not be empty when restriction is enabled
        RuleFor(x => x.AllowedEnvironments)
            .NotEmpty()
            .When(x => x.RestrictByEnvironment)
            .WithMessage("Allowed environments cannot be empty when restriction is enabled.");

        // ==================== METADATA ====================

        RuleFor(x => x.ChangeDescription)
            .MaximumLength(1000)
            .WithMessage("Change description cannot exceed 1000 characters.");
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
