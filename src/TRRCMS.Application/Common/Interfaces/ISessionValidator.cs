namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Validates on every authenticated request that the JWT still represents a live session:
/// the user exists, is active, and the token's security stamp matches the current one.
/// The stamp is rotated by password change/reset and 2FA changes, which ends all existing sessions.
/// </summary>
public interface ISessionValidator
{
    /// <summary>
    /// Returns null when the session is valid, otherwise the ErrorMessages resource key
    /// explaining why it was rejected.
    /// </summary>
    Task<string?> ValidateAsync(Guid userId, string? securityStamp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Drop the cached session state of a user so the next request re-reads it
    /// (call after rotating the security stamp or deactivating the user).
    /// </summary>
    void Invalidate(Guid userId);
}
