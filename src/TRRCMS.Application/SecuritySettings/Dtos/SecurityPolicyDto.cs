namespace TRRCMS.Application.SecuritySettings.Dtos;

/// <summary>
/// DTO representing a complete security policy version.
/// Returned by GET endpoints and after successful updates.
/// </summary>
public class SecurityPolicyDto
{
    /// <summary>Unique ID of this policy version</summary>
    public Guid Id { get; set; }

    /// <summary>Policy version number (increments with each apply)</summary>
    public int Version { get; set; }

    /// <summary>Whether this is the currently enforced policy</summary>
    public bool IsActive { get; set; }

    /// <summary>When this policy version became effective</summary>
    public DateTime EffectiveFromUtc { get; set; }

    /// <summary>When this policy version was superseded (null if still active)</summary>
    public DateTime? EffectiveToUtc { get; set; }

    /// <summary>Description of changes in this version</summary>
    public string? ChangeDescription { get; set; }

    /// <summary>User ID who applied this policy</summary>
    public Guid AppliedByUserId { get; set; }

    // ==================== NESTED POLICY SECTIONS ====================

    public PasswordPolicyDto PasswordPolicy { get; set; } = null!;

    public SessionLockoutPolicyDto SessionLockoutPolicy { get; set; } = null!;

    public AccessControlPolicyDto AccessControlPolicy { get; set; } = null!;

    // ==================== AUDIT ====================

    public DateTime CreatedAtUtc { get; set; }

    public Guid CreatedBy { get; set; }
}

/// <summary>
/// DTO for password policy section (UC-011 S03).
/// </summary>
public class PasswordPolicyDto
{
    /// <summary>Minimum password length (8–128)</summary>
    public int MinLength { get; set; }

    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireDigit { get; set; }
    public bool RequireSpecialCharacter { get; set; }

    /// <summary>Days until password expires (0 = never)</summary>
    public int ExpiryDays { get; set; }

    /// <summary>Number of previous passwords blocked from reuse (0 = none)</summary>
    public int ReuseHistory { get; set; }
}

/// <summary>
/// DTO for session and lockout policy section (UC-011 S04).
/// </summary>
public class SessionLockoutPolicyDto
{
    /// <summary>Session inactivity timeout in minutes (5–1440)</summary>
    public int SessionTimeoutMinutes { get; set; }

    /// <summary>Max failed login attempts before lockout (3–20)</summary>
    public int MaxFailedLoginAttempts { get; set; }

    /// <summary>Account lockout duration in minutes (1–1440)</summary>
    public int LockoutDurationMinutes { get; set; }
}

/// <summary>
/// DTO for access control policy section (UC-011 S05).
/// </summary>
public class AccessControlPolicyDto
{
    public bool AllowPasswordAuthentication { get; set; }
    public bool AllowSsoAuthentication { get; set; }
    public bool AllowTokenAuthentication { get; set; }

    public bool EnforceIpAllowlist { get; set; }
    public string? IpAllowlist { get; set; }
    public string? IpDenylist { get; set; }

    public bool RestrictByEnvironment { get; set; }
    public string? AllowedEnvironments { get; set; }
}
