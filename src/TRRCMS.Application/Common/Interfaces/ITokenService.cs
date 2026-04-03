using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate a JWT access token for a user
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <param name="deviceId">Optional device identifier for audit trail</param>
    /// <param name="isPasswordChangeOnly">When true, generates a restricted token with must_change_password claim and shorter expiry</param>
    /// <returns>JWT access token (short-lived, 15 minutes; or 10 minutes for password-change-only)</returns>
    string GenerateAccessToken(User user, string? deviceId = null, bool isPasswordChangeOnly = false);

    /// <summary>
    /// Generate a refresh token (random string)
    /// </summary>
    /// <returns>Refresh token (long-lived, 7 days)</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Get user ID from JWT token
    /// </summary>
    /// <param name="token">JWT access token</param>
    /// <returns>User ID if token is valid, null otherwise</returns>
    Guid? GetUserIdFromToken(string token);

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <param name="token">JWT access token</param>
    /// <returns>True if token is valid and not expired, false otherwise</returns>
    bool ValidateToken(string token);
}