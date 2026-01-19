using MediatR;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.GrantPermissions;

/// <summary>
/// Grant multiple permissions to a user command
/// UC-009: User & Role Management - Manage Roles and Permissions
/// </summary>
public class GrantPermissionsCommand : IRequest<UserDto>
{
    /// <summary>
    /// User ID to grant permissions to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// List of permissions to grant
    /// </summary>
    public List<Permission> Permissions { get; set; } = new();

    /// <summary>
    /// Reason for granting permissions (required for audit trail)
    /// </summary>
    public string GrantReason { get; set; } = string.Empty;
}