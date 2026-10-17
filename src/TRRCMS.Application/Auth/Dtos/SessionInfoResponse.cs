namespace TRRCMS.Application.Auth.Dtos;

/// <summary>
/// Current session timing, for clients that need to (re)schedule their expiry warning
/// </summary>
public class SessionInfoResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// When the access token used for this request expires (UTC)
    /// </summary>
    public DateTime AccessTokenExpiry { get; set; }

    /// <summary>
    /// Server clock at response time (UTC)
    /// </summary>
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Seconds left on the access token by the server clock — schedule timers from this, not from the client clock
    /// </summary>
    public int AccessTokenExpiresInSeconds => Math.Max(0, (int)(AccessTokenExpiry - ServerTimeUtc).TotalSeconds);

    /// <summary>
    /// Show the "session about to expire" warning this many seconds before expiry
    /// </summary>
    public int SessionExpiryWarningSeconds { get; set; }

    /// <summary>
    /// True for the restricted token issued when the user must change their password
    /// </summary>
    public bool MustChangePassword { get; set; }
}
