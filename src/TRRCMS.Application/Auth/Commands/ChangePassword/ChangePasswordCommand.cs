using MediatR;
using TRRCMS.Application.Auth.Dtos;

namespace TRRCMS.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Command to change a user's password
/// </summary>
public class ChangePasswordCommand : IRequest<ChangePasswordResponse>
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public Guid ModifiedByUserId { get; set; }

    /// <summary>
    /// Device id of the caller's current token (tablets), carried into the fresh token pair
    /// </summary>
    public string? DeviceId { get; set; }

    public ChangePasswordCommand(
        Guid userId,
        string currentPassword,
        string newPassword,
        string confirmPassword,
        Guid modifiedByUserId)
    {
        UserId = userId;
        CurrentPassword = currentPassword;
        NewPassword = newPassword;
        ConfirmPassword = confirmPassword;
        ModifiedByUserId = modifiedByUserId;
    }
}