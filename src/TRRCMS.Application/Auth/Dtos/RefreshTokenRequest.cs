namespace TRRCMS.Application.Auth.Dtos;

/// <summary>
/// Request model for refreshing access token
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Refresh token to use for generating new access token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional device identifier for audit trail
    /// </summary>
    public string? DeviceId { get; set; }
}