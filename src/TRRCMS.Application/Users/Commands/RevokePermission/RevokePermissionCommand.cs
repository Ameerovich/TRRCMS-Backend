using MediatR;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.RevokePermission;

/// <summary>
/// Revoke a single permission from a user command
/// UC-009: User & Role Management - Manage Roles and Permissions
/// </summary>
public class RevokePermissionCommand : IRequest<UserDto>
{
    /// <summary>
    /// User ID to revoke permission from
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Permission to revoke
    /// </summary>
    public Permission Permission { get; set; }

    /// <summary>
    /// Reason for revoking permission (required for audit trail)
    /// </summary>
    public string RevokeReason { get; set; } = string.Empty;
}