using MediatR;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Commands.UnlockUser;

/// <summary>
/// Unlock user account command
/// UC-009: User & Role Management
/// Clears failed login attempts and removes lockout
/// </summary>
public class UnlockUserCommand : IRequest<UserDto>
{
    /// <summary>
    /// User ID to unlock
    /// </summary>
    public Guid UserId { get; set; }
}