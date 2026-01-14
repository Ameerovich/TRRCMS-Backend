using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that checks if the current user has required permissions
/// Integrates with User.HasPermission() domain logic
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        ICurrentUserService currentUserService,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Check if user is authenticated
        if (!_currentUserService.IsAuthenticated)
        {
            _logger.LogWarning("Authorization failed: User is not authenticated");
            context.Fail(new AuthorizationFailureReason(this, "User is not authenticated"));
            return;
        }

        // Get current user with permissions
        var user = await _currentUserService.GetCurrentUserAsync();

        if (user == null)
        {
            _logger.LogWarning(
                "Authorization failed: User ID {UserId} not found in database",
                _currentUserService.UserId);
            context.Fail(new AuthorizationFailureReason(this, "User not found"));
            return;
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Authorization failed: User {Username} (ID: {UserId}) is not active",
                user.Username,
                user.Id);
            context.Fail(new AuthorizationFailureReason(this, "User account is not active"));
            return;
        }

        // Check if user is locked out
        if (user.IsLockedOut && !user.IsLockoutExpired())
        {
            _logger.LogWarning(
                "Authorization failed: User {Username} (ID: {UserId}) is locked out until {LockoutEnd}",
                user.Username,
                user.Id,
                user.LockoutEndDate);
            context.Fail(new AuthorizationFailureReason(this, "User account is locked"));
            return;
        }

        // Check permission requirements
        bool hasPermission = false;

        if (requirement.AllPermissions != null && requirement.AllPermissions.Length > 0)
        {
            // Require ALL permissions
            hasPermission = user.HasAllPermissions(requirement.AllPermissions);

            if (!hasPermission)
            {
                _logger.LogWarning(
                    "Authorization failed: User {Username} (ID: {UserId}) does not have ALL required permissions: {RequiredPermissions}. User has: {UserPermissions}",
                    user.Username,
                    user.Id,
                    string.Join(", ", requirement.AllPermissions),
                    string.Join(", ", user.GetActivePermissions()));
            }
        }
        else if (requirement.AnyPermissions != null && requirement.AnyPermissions.Length > 0)
        {
            // Require ANY permission
            hasPermission = user.HasAnyPermission(requirement.AnyPermissions);

            if (!hasPermission)
            {
                _logger.LogWarning(
                    "Authorization failed: User {Username} (ID: {UserId}) does not have ANY required permissions: {RequiredPermissions}. User has: {UserPermissions}",
                    user.Username,
                    user.Id,
                    string.Join(", ", requirement.AnyPermissions),
                    string.Join(", ", user.GetActivePermissions()));
            }
        }
        else
        {
            // Require single permission
            hasPermission = user.HasPermission(requirement.Permission);

            if (!hasPermission)
            {
                _logger.LogWarning(
                    "Authorization failed: User {Username} (ID: {UserId}, Role: {Role}) does not have permission {RequiredPermission}. User has: {UserPermissions}",
                    user.Username,
                    user.Id,
                    user.Role,
                    requirement.Permission,
                    string.Join(", ", user.GetActivePermissions()));
            }
        }

        if (hasPermission)
        {
            _logger.LogInformation(
                "Authorization succeeded: User {Username} (ID: {UserId}) has required permission(s)",
                user.Username,
                user.Id);
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                $"User does not have required permission: {requirement.GetDescription()}"));
        }
    }
}
