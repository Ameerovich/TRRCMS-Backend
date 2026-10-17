using Microsoft.Extensions.Configuration;

namespace TRRCMS.Application.Auth;

/// <summary>
/// Client-facing session timing settings.
/// </summary>
public static class SessionSettings
{
    public const int DefaultExpiryWarningSeconds = 60;

    /// <summary>
    /// How many seconds before the access token expires clients should warn the user and offer to renew
    /// (JwtSettings:SessionExpiryWarningSeconds, default 60).
    /// </summary>
    public static int GetExpiryWarningSeconds(IConfiguration configuration)
        => int.TryParse(configuration["JwtSettings:SessionExpiryWarningSeconds"], out var seconds) && seconds > 0
            ? seconds
            : DefaultExpiryWarningSeconds;
}
