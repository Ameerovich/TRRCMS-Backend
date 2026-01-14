using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service to get the current authenticated user
/// Used for authorization and audit trail
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the current user's ID from the HTTP context
    /// Returns null if no user is authenticated
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Get the current user's username
    /// Returns null if no user is authenticated
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Get the full User entity with permissions loaded
    /// Returns null if no user is authenticated
    /// </summary>
    Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Get the current user's IP address
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Get the current user's device/application source
    /// </summary>
    string? SourceApplication { get; }
}
