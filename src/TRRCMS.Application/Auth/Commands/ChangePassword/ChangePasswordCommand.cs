using MediatR;

namespace TRRCMS.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Command to change a user's password
/// </summary>
public class ChangePasswordCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public Guid ModifiedByUserId { get; set; }

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