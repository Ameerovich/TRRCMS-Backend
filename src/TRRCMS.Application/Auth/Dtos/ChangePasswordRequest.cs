namespace TRRCMS.Application.Auth.Dtos;

/// <summary>
/// Request model for changing password
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// User ID whose password is being changed
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Current password (for verification)
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation of new password (must match NewPassword)
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}