using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Application.Common.Security;

/// <summary>
/// Single source of truth for checking a password against the active <see cref="PasswordPolicy"/>
/// and for generating temporary passwords that satisfy it.
/// </summary>
public static class PasswordPolicyRules
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Special = "!@#$%*?-_";

    /// <summary>
    /// Returns the ValidationMessages resource keys (with format args) for every rule the password breaks.
    /// </summary>
    public static IEnumerable<(string Key, object[] Args)> GetViolations(string password, PasswordPolicy policy)
    {
        if (password.Length < policy.MinLength)
            yield return ("Password_MinLength", new object[] { policy.MinLength });

        if (policy.RequireUppercase && !Regex.IsMatch(password, "[A-Z]"))
            yield return ("Password_RequireUpper", Array.Empty<object>());

        if (policy.RequireLowercase && !Regex.IsMatch(password, "[a-z]"))
            yield return ("Password_RequireLower", Array.Empty<object>());

        if (policy.RequireDigit && !Regex.IsMatch(password, "[0-9]"))
            yield return ("Password_RequireDigit", Array.Empty<object>());

        if (policy.RequireSpecialCharacter && !Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            yield return ("Password_RequireSpecial", Array.Empty<object>());
    }

    /// <summary>
    /// Generates a random temporary password that always satisfies <paramref name="policy"/>.
    /// Ambiguous characters (0/O, 1/l/I) are excluded so it can be read out over the phone.
    /// </summary>
    public static string GenerateTemporaryPassword(PasswordPolicy policy)
    {
        var length = Math.Max(policy.MinLength, 12);
        var chars = new List<char>
        {
            Pick(Upper), Pick(Lower), Pick(Digits), Pick(Special)
        };

        const string all = Upper + Lower + Digits + Special;
        while (chars.Count < length)
            chars.Add(Pick(all));

        // Fisher–Yates shuffle so the guaranteed classes are not always at the start
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }

    private static char Pick(string alphabet) => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
}
