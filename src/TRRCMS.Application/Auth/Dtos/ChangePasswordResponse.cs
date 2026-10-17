namespace TRRCMS.Application.Auth.Dtos;

/// <summary>
/// Response of POST /api/v1|v2/auth/change-password.
/// Changing the password ends every existing session of the user; the caller's own session
/// continues with the fresh token pair returned here (same fields as /auth/refresh).
/// </summary>
public class ChangePasswordResponse : RefreshTokenResponse
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
}
