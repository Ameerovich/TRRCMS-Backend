using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// BCrypt-based password hashing service
/// BCrypt automatically handles salt generation and embeds it in the hash
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    // BCrypt work factor (number of hashing rounds)
    // 12 = 2^12 rounds (4096) - good balance between security and performance
    private const int WorkFactor = 12;

    /// <summary>
    /// Hash a password using BCrypt
    /// Note: BCrypt embeds the salt in the hash, but we also return it separately
    /// for compatibility with the User entity structure
    /// </summary>
    public string HashPassword(string password, out string salt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        // Generate salt using BCrypt
        salt = BCrypt.Net.BCrypt.GenerateSalt(WorkFactor);

        // Hash password with the generated salt
        string hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

        return hash;
    }

    /// <summary>
    /// Verify a password against a BCrypt hash
    /// Note: BCrypt.Verify() internally handles salt extraction from the hash,
    /// but we keep the salt parameter for interface compatibility
    /// </summary>
    public bool VerifyPassword(string password, string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            Console.WriteLine("=== PASSWORD VERIFICATION ===");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Hash: {hash.Substring(0, Math.Min(30, hash.Length))}...");
            Console.WriteLine($"Hash Length: {hash.Length}");

            bool result = BCrypt.Net.BCrypt.Verify(password, hash);

            Console.WriteLine($"Verification Result: {result}");
            Console.WriteLine("=========================");

            return result;
        }
        catch (Exception ex)
        {
            // ⚠️ NOW WE CAN SEE THE ERROR!
            Console.WriteLine("=== BCrypt VERIFICATION ERROR ===");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"Exception Message: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            Console.WriteLine("================================");
            return false;
        }
    }
}