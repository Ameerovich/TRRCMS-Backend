namespace TRRCMS.Domain.ValueObjects;

/// <summary>
/// Value object encapsulating session timeout and account lockout parameters.
/// UC-011 S04: Configure Session Timeout and Lockout Settings.
/// 
/// Safety constraints prevent configurations that would make the system unusable:
///   - SessionTimeoutMinutes ∈ [5, 1440] (5 min to 24 hours)
///   - MaxFailedLoginAttempts ∈ [3, 20]
///   - LockoutDurationMinutes ∈ [1, 1440]
/// </summary>
public sealed class SessionLockoutPolicy : IEquatable<SessionLockoutPolicy>
{
    // ==================== SAFETY BOUNDS ====================
    public const int MinSessionTimeout = 5;
    public const int MaxSessionTimeout = 1440; // 24 hours
    public const int MinFailedAttempts = 3;
    public const int MaxFailedAttempts = 20;
    public const int MinLockoutDuration = 1;
    public const int MaxLockoutDuration = 1440; // 24 hours

    /// <summary>Session inactivity timeout in minutes (default: 30)</summary>
    public int SessionTimeoutMinutes { get; private set; }

    /// <summary>Maximum consecutive failed login attempts before lockout (default: 5)</summary>
    public int MaxFailedLoginAttempts { get; private set; }

    /// <summary>Account lockout duration in minutes after exceeding failed attempts (default: 15)</summary>
    public int LockoutDurationMinutes { get; private set; }

    // EF Core requires a parameterless constructor
    private SessionLockoutPolicy() { }

    private SessionLockoutPolicy(
        int sessionTimeoutMinutes,
        int maxFailedLoginAttempts,
        int lockoutDurationMinutes)
    {
        SessionTimeoutMinutes = sessionTimeoutMinutes;
        MaxFailedLoginAttempts = maxFailedLoginAttempts;
        LockoutDurationMinutes = lockoutDurationMinutes;
    }

    /// <summary>
    /// Factory method with invariant validation.
    /// Ensures parameters stay within safe operational bounds.
    /// </summary>
    public static SessionLockoutPolicy Create(
        int sessionTimeoutMinutes,
        int maxFailedLoginAttempts,
        int lockoutDurationMinutes)
    {
        if (sessionTimeoutMinutes < MinSessionTimeout || sessionTimeoutMinutes > MaxSessionTimeout)
            throw new ArgumentException(
                $"Session timeout must be between {MinSessionTimeout} and {MaxSessionTimeout} minutes.",
                nameof(sessionTimeoutMinutes));

        if (maxFailedLoginAttempts < MinFailedAttempts || maxFailedLoginAttempts > MaxFailedAttempts)
            throw new ArgumentException(
                $"Maximum failed login attempts must be between {MinFailedAttempts} and {MaxFailedAttempts}.",
                nameof(maxFailedLoginAttempts));

        if (lockoutDurationMinutes < MinLockoutDuration || lockoutDurationMinutes > MaxLockoutDuration)
            throw new ArgumentException(
                $"Lockout duration must be between {MinLockoutDuration} and {MaxLockoutDuration} minutes.",
                nameof(lockoutDurationMinutes));

        return new SessionLockoutPolicy(sessionTimeoutMinutes, maxFailedLoginAttempts, lockoutDurationMinutes);
    }

    /// <summary>
    /// Returns the default session/lockout policy matching FSD Section 13 recommendations.
    /// </summary>
    public static SessionLockoutPolicy Default() => new(
        sessionTimeoutMinutes: 30,
        maxFailedLoginAttempts: 5,
        lockoutDurationMinutes: 15);

    // ==================== EQUALITY ====================

    public bool Equals(SessionLockoutPolicy? other)
    {
        if (other is null) return false;
        return SessionTimeoutMinutes == other.SessionTimeoutMinutes
            && MaxFailedLoginAttempts == other.MaxFailedLoginAttempts
            && LockoutDurationMinutes == other.LockoutDurationMinutes;
    }

    public override bool Equals(object? obj) => Equals(obj as SessionLockoutPolicy);

    public override int GetHashCode() => HashCode.Combine(
        SessionTimeoutMinutes, MaxFailedLoginAttempts, LockoutDurationMinutes);
}
