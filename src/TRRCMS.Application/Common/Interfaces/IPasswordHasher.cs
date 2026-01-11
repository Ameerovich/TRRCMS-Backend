namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service for securely hashing and verifying passwords
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a password using BCrypt
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="salt">Salt to use (output parameter)</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password, out string salt);

    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    /// <param name="password">Plain text password to verify</param>
    /// <param name="hash">Stored password hash</param>
    /// <param name="salt">Stored salt (for compatibility, BCrypt embeds salt in hash)</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string password, string hash, string salt);
}