using MediatR;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Commands.ResetUserPassword;

/// <summary>
/// Administrator resets another user's password to a temporary one.
/// The user must change it at next login.
/// </summary>
public class ResetUserPasswordCommand : IRequest<ResetUserPasswordResultDto>
{
    /// <summary>
    /// User whose password is reset
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Optional temporary password chosen by the administrator.
    /// When omitted the server generates one that satisfies the active password policy
    /// and returns it once in the response.
    /// </summary>
    public string? TemporaryPassword { get; set; }
}
