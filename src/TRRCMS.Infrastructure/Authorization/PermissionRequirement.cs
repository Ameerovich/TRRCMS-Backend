using Microsoft.AspNetCore.Authorization;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Authorization;

/// <summary>
/// Custom authorization requirement that checks if user has a specific permission
/// Used with PermissionAuthorizationHandler to enforce permission-based access
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// The permission required to access the resource
    /// </summary>
    public Permission Permission { get; }

    /// <summary>
    /// Optional: Require user to have ALL of these permissions
    /// If null or empty, only checks the single Permission property
    /// </summary>
    public Permission[]? AllPermissions { get; private set; }

    /// <summary>
    /// Optional: Require user to have ANY of these permissions
    /// If null or empty, only checks the single Permission property
    /// </summary>
    public Permission[]? AnyPermissions { get; private set; }

    /// <summary>
    /// Create a requirement for a single permission
    /// </summary>
    public PermissionRequirement(Permission permission)
    {
        Permission = permission;
    }

    /// <summary>
    /// Create a requirement for ALL specified permissions
    /// </summary>
    public static PermissionRequirement RequireAll(params Permission[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("At least one permission must be specified", nameof(permissions));

        return new PermissionRequirement(permissions[0])
        {
            AllPermissions = permissions
        };
    }

    /// <summary>
    /// Create a requirement for ANY of the specified permissions
    /// </summary>
    public static PermissionRequirement RequireAny(params Permission[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("At least one permission must be specified", nameof(permissions));

        return new PermissionRequirement(permissions[0])
        {
            AnyPermissions = permissions
        };
    }

    /// <summary>
    /// Get a human-readable description of this requirement
    /// Useful for error messages and logging
    /// </summary>
    public string GetDescription()
    {
        if (AllPermissions != null && AllPermissions.Length > 0)
        {
            return $"Requires ALL permissions: {string.Join(", ", AllPermissions.Select(p => p.ToString()))}";
        }

        if (AnyPermissions != null && AnyPermissions.Length > 0)
        {
            return $"Requires ANY permission: {string.Join(", ", AnyPermissions.Select(p => p.ToString()))}";
        }

        return $"Requires permission: {Permission}";
    }
}
