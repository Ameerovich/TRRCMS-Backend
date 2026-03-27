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
    /// Optional. Only used by field collectors on tablets for device tracking.
    /// Desktop and dashboard clients should omit this field.
    /// </summary>
    public string? DeviceId { get; set; }
}