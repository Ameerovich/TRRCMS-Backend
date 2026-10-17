namespace TRRCMS.Application.Auth.Dtos;

/// <summary>
/// Request model for POST /api/v1/auth/forgot-password
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Username of the account that needs a password reset
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Optional note for the administrator (e.g. phone number or office), max 500 characters
    /// </summary>
    public string? Note { get; set; }
}
