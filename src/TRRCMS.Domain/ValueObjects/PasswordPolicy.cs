namespace TRRCMS.Domain.ValueObjects;

/// <summary>
/// Value object encapsulating password policy parameters.
/// UC-011 S03: Configure Password Policies.
/// FSD Section 13.1: Security Requirements.
/// 
/// Enforces organizational minimums:
///   - MinLength ∈ [8, 128]
///   - ExpiryDays ∈ [0 (disabled), 365]
///   - ReuseHistory ∈ [0 (disabled), 24]
/// </summary>
public sealed class PasswordPolicy : IEquatable<PasswordPolicy>
{
    // ==================== ORGANIZATIONAL MINIMUMS ====================
    public const int AbsoluteMinLength = 8;
    public const int AbsoluteMaxLength = 128;
    public const int MaxExpiryDays = 365;
    public const int MaxReuseHistory = 24;

    /// <summary>Minimum password length (default: 8)</summary>
    public int MinLength { get; private set; }

    /// <summary>Require at least one uppercase letter</summary>
    public bool RequireUppercase { get; private set; }

    /// <summary>Require at least one lowercase letter</summary>
    public bool RequireLowercase { get; private set; }

    /// <summary>Require at least one digit</summary>
    public bool RequireDigit { get; private set; }

    /// <summary>Require at least one special character</summary>
    public bool RequireSpecialCharacter { get; private set; }

    /// <summary>Days until password expires (0 = never expires)</summary>
    public int ExpiryDays { get; private set; }

    /// <summary>Number of previous passwords that cannot be reused (0 = no restriction)</summary>
    public int ReuseHistory { get; private set; }

    // EF Core requires a parameterless constructor
    private PasswordPolicy() { }

    private PasswordPolicy(
        int minLength,
        bool requireUppercase,
        bool requireLowercase,
        bool requireDigit,
        bool requireSpecialCharacter,
        int expiryDays,
        int reuseHistory)
    {
        MinLength = minLength;
        RequireUppercase = requireUppercase;
        RequireLowercase = requireLowercase;
        RequireDigit = requireDigit;
        RequireSpecialCharacter = requireSpecialCharacter;
        ExpiryDays = expiryDays;
        ReuseHistory = reuseHistory;
    }

    /// <summary>
    /// Factory method with full invariant validation.
    /// Throws <see cref="ArgumentException"/> if any parameter violates safety constraints.
    /// </summary>
    public static PasswordPolicy Create(
        int minLength,
        bool requireUppercase,
        bool requireLowercase,
        bool requireDigit,
        bool requireSpecialCharacter,
        int expiryDays,
        int reuseHistory)
    {
        if (minLength < AbsoluteMinLength || minLength > AbsoluteMaxLength)
            throw new ArgumentException(
                $"Password minimum length must be between {AbsoluteMinLength} and {AbsoluteMaxLength}.",
                nameof(minLength));

        if (expiryDays < 0 || expiryDays > MaxExpiryDays)
            throw new ArgumentException(
                $"Password expiry days must be between 0 (disabled) and {MaxExpiryDays}.",
                nameof(expiryDays));

        if (reuseHistory < 0 || reuseHistory > MaxReuseHistory)
            throw new ArgumentException(
                $"Password reuse history must be between 0 (disabled) and {MaxReuseHistory}.",
                nameof(reuseHistory));

        return new PasswordPolicy(
            minLength, requireUppercase, requireLowercase,
            requireDigit, requireSpecialCharacter,
            expiryDays, reuseHistory);
    }

    /// <summary>
    /// Returns the default (baseline) password policy matching FSD Section 13 recommendations.
    /// </summary>
    public static PasswordPolicy Default() => new(
        minLength: 8,
        requireUppercase: true,
        requireLowercase: true,
        requireDigit: true,
        requireSpecialCharacter: true,
        expiryDays: 90,
        reuseHistory: 5);

    // ==================== EQUALITY ====================

    public bool Equals(PasswordPolicy? other)
    {
        if (other is null) return false;
        return MinLength == other.MinLength
            && RequireUppercase == other.RequireUppercase
            && RequireLowercase == other.RequireLowercase
            && RequireDigit == other.RequireDigit
            && RequireSpecialCharacter == other.RequireSpecialCharacter
            && ExpiryDays == other.ExpiryDays
            && ReuseHistory == other.ReuseHistory;
    }

    public override bool Equals(object? obj) => Equals(obj as PasswordPolicy);

    public override int GetHashCode() => HashCode.Combine(
        MinLength, RequireUppercase, RequireLowercase,
        RequireDigit, RequireSpecialCharacter, ExpiryDays, ReuseHistory);
}
