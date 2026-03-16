using MediatR;
using TRRCMS.Application.SecuritySettings.Dtos;

namespace TRRCMS.Application.SecuritySettings.Commands.UpdateSecuritySettings;

/// <summary>
/// Command to validate and apply a new security policy configuration.
/// Creates a new versioned SecurityPolicy record, deactivates the previous active policy, and logs the change.
/// </summary>
public class UpdateSecuritySettingsCommand : IRequest<SecurityPolicyDto>
{
    // Password policy

    /// <summary>Minimum password length (default: 8, range: 8–128)</summary>
    public int PasswordMinLength { get; set; } = 8;

    /// <summary>Require at least one uppercase letter</summary>
    public bool PasswordRequireUppercase { get; set; } = true;

    /// <summary>Require at least one lowercase letter</summary>
    public bool PasswordRequireLowercase { get; set; } = true;

    /// <summary>Require at least one digit</summary>
    public bool PasswordRequireDigit { get; set; } = true;

    /// <summary>Require at least one special character</summary>
    public bool PasswordRequireSpecialCharacter { get; set; } = true;

    /// <summary>Days until password expires (0 = never expires)</summary>
    public int PasswordExpiryDays { get; set; } = 90;

    /// <summary>Number of previous passwords blocked from reuse (0 = no restriction)</summary>
    public int PasswordReuseHistory { get; set; } = 5;

    // Session and lockout policy

    /// <summary>Session inactivity timeout in minutes (range: 5–1440)</summary>
    public int SessionTimeoutMinutes { get; set; } = 30;

    /// <summary>Max consecutive failed login attempts before lockout (range: 3–20)</summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>Account lockout duration in minutes (range: 1–1440)</summary>
    public int LockoutDurationMinutes { get; set; } = 15;

    // Access control policy

    /// <summary>Allow username/password authentication</summary>
    public bool AllowPasswordAuthentication { get; set; } = true;

    /// <summary>Allow SSO/SAML authentication</summary>
    public bool AllowSsoAuthentication { get; set; } = false;

    /// <summary>Allow token-based API authentication</summary>
    public bool AllowTokenAuthentication { get; set; } = true;

    /// <summary>Enforce IP allowlist for admin access</summary>
    public bool EnforceIpAllowlist { get; set; } = false;

    /// <summary>Comma-separated allowed IPs/CIDR (e.g., "192.168.1.0/24,10.0.0.1")</summary>
    public string? IpAllowlist { get; set; }

    /// <summary>Comma-separated denied IPs/CIDR</summary>
    public string? IpDenylist { get; set; }

    /// <summary>Restrict access by environment type</summary>
    public bool RestrictByEnvironment { get; set; } = false;

    /// <summary>Comma-separated allowed environments (e.g., "Desktop,Mobile")</summary>
    public string? AllowedEnvironments { get; set; }

    // Metadata

    /// <summary>Human-readable description of what changed and why</summary>
    public string? ChangeDescription { get; set; }
}
