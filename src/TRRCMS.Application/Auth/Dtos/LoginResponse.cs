using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Auth.Dtos;

/// <summary>
/// Response model for successful login
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Full name in Arabic
    /// </summary>
    public string FullNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// Full name in English (optional)
    /// </summary>
    public string? FullNameEnglish { get; set; }

    /// <summary>
    /// Email address (optional)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User role
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Role name as string
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Whether user has mobile app access
    /// </summary>
    public bool HasMobileAccess { get; set; }

    /// <summary>
    /// Whether user has desktop app access
    /// </summary>
    public bool HasDesktopAccess { get; set; }

    /// <summary>
    /// Preferred language
    /// </summary>
    public string PreferredLanguage { get; set; } = string.Empty;

    /// <summary>
    /// JWT access token (short-lived, 15 minutes)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token (long-lived, 7 days). Null when MustChangePassword is true.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Access token expiration time (UTC)
    /// </summary>
    public DateTime AccessTokenExpiry { get; set; }

    /// <summary>
    /// Refresh token expiration time (UTC). Null when MustChangePassword is true.
    /// </summary>
    public DateTime? RefreshTokenExpiry { get; set; }

    /// <summary>
    /// Whether user must change password on next login
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>
    /// Server clock when this response was produced (UTC). Together with
    /// <see cref="AccessTokenExpiresInSeconds"/> lets clients schedule the expiry warning
    /// without trusting their own clock (devices are often skewed).
    /// </summary>
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Seconds until the access token expires, by the server clock
    /// </summary>
    public int AccessTokenExpiresInSeconds => Math.Max(0, (int)(AccessTokenExpiry - ServerTimeUtc).TotalSeconds);

    /// <summary>
    /// Show the "session about to expire — renew?" warning this many seconds before expiry
    /// </summary>
    public int SessionExpiryWarningSeconds { get; set; } = SessionSettings.DefaultExpiryWarningSeconds;
}