namespace TRRCMS.Application.Auth.Dtos;

/// <summary>
/// Response model for token refresh
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>
    /// New JWT access token (short-lived, 15 minutes)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// New refresh token (long-lived, 7 days)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration time (UTC)
    /// </summary>
    public DateTime AccessTokenExpiry { get; set; }

    /// <summary>
    /// Refresh token expiration time (UTC)
    /// </summary>
    public DateTime RefreshTokenExpiry { get; set; }

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