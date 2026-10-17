namespace TRRCMS.Application.Users.Dtos;

/// <summary>
/// Optional body for POST /api/v1/users/{id}/reset-password
/// </summary>
public class ResetUserPasswordRequest
{
    /// <summary>
    /// Temporary password chosen by the administrator. Omit to let the server generate one.
    /// </summary>
    public string? TemporaryPassword { get; set; }
}

/// <summary>
/// Result of an administrator password reset
/// </summary>
public class ResetUserPasswordResultDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The temporary password, returned only when the server generated it.
    /// Null when the administrator supplied their own. Never stored or retrievable again.
    /// </summary>
    public string? GeneratedTemporaryPassword { get; set; }

    /// <summary>
    /// Always true: the user is forced to choose a new password at next login
    /// </summary>
    public bool MustChangePassword { get; set; }

    public DateTime ResetAtUtc { get; set; }
}

/// <summary>
/// A pending "forgot password" request waiting for an administrator
/// </summary>
public class PasswordResetRequestDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullNameArabic { get; set; } = string.Empty;
    public string? FullNameEnglish { get; set; }
    public int Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public string? Note { get; set; }
}
