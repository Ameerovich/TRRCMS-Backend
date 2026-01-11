namespace TRRCMS.Application.Auth.Dtos;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username for authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional device identifier for audit trail (tablet ID, device name, etc.)
    /// Required by FSD for tracking which device initiated the login
    /// </summary>
    public string? DeviceId { get; set; }
}