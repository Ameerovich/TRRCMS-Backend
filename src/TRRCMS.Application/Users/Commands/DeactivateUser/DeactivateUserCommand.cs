using MediatR;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Commands.DeactivateUser;

/// <summary>
/// Deactivate user account command
/// UC-009: User & Role Management
/// </summary>
public class DeactivateUserCommand : IRequest<UserDto>
{
    /// <summary>
    /// User ID to deactivate
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Reason for deactivation (required for audit trail)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}