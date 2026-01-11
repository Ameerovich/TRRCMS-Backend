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
}