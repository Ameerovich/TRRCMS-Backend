using TRRCMS.Domain.Common;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Aggregate root representing a versioned system security policy configuration.
///
/// Design decisions:
///   - Each "Apply" creates a new SecurityPolicy record with an incremented version.
///   - Only one policy is active at a time (<see cref="IsActive"/> = true).
///   - Previous versions are preserved for full audit trail.
///   - Composed of three value objects: PasswordPolicy, SessionLockoutPolicy, AccessControlPolicy.
///   - Domain invariants are enforced in the value objects and in <see cref="Validate"/>.
/// </summary>
public class SecurityPolicy : BaseAuditableEntity
{
    /// <summary>Policy version number (auto-incremented on each apply)</summary>
    public int Version { get; private set; }

    /// <summary>Whether this is the currently active/enforced policy</summary>
    public bool IsActive { get; private set; }

    /// <summary>UTC timestamp when this policy version became effective</summary>
    public DateTime EffectiveFromUtc { get; private set; }

    /// <summary>UTC timestamp when this policy version was superseded (null if still active)</summary>
    public DateTime? EffectiveToUtc { get; private set; }
    /// <summary>Password complexity and expiry rules</summary>
    public PasswordPolicy PasswordPolicy { get; private set; } = null!;

    /// <summary>Session timeout and account lockout rules</summary>
    public SessionLockoutPolicy SessionLockoutPolicy { get; private set; } = null!;

    /// <summary>Authentication methods and IP restrictions</summary>
    public AccessControlPolicy AccessControlPolicy { get; private set; } = null!;
    /// <summary>Human-readable description of changes in this version</summary>
    public string? ChangeDescription { get; private set; }

    /// <summary>User ID who approved and applied this policy</summary>
    public Guid AppliedByUserId { get; private set; }
    // EF Core requires a parameterless constructor
    private SecurityPolicy() : base() { }

    private SecurityPolicy(
        int version,
        PasswordPolicy passwordPolicy,
        SessionLockoutPolicy sessionLockoutPolicy,
        AccessControlPolicy accessControlPolicy,
        string? changeDescription,
        Guid appliedByUserId) : base()
    {
        Version = version;
        IsActive = true;
        EffectiveFromUtc = DateTime.UtcNow;
        PasswordPolicy = passwordPolicy;
        SessionLockoutPolicy = sessionLockoutPolicy;
        AccessControlPolicy = accessControlPolicy;
        ChangeDescription = changeDescription;
        AppliedByUserId = appliedByUserId;

        MarkAsCreated(appliedByUserId);
    }
    /// <summary>
    /// Creates the initial (v1) security policy with default settings.
    /// Called once during system seeding.
    /// </summary>
    public static SecurityPolicy CreateDefault(Guid systemUserId)
    {
        return new SecurityPolicy(
            version: 1,
            passwordPolicy: PasswordPolicy.Default(),
            sessionLockoutPolicy: SessionLockoutPolicy.Default(),
            accessControlPolicy: AccessControlPolicy.Default(),
            changeDescription: "Initial default security policy",
            appliedByUserId: systemUserId);
    }

    /// <summary>
    /// Creates a new policy version from updated settings.
    /// The caller is responsible for deactivating the previous version first.
    /// </summary>
    public static SecurityPolicy CreateNewVersion(
        int nextVersion,
        PasswordPolicy passwordPolicy,
        SessionLockoutPolicy sessionLockoutPolicy,
        AccessControlPolicy accessControlPolicy,
        string? changeDescription,
        Guid appliedByUserId)
    {
        if (nextVersion < 2)
            throw new ArgumentException("New version must be >= 2.", nameof(nextVersion));

        var policy = new SecurityPolicy(
            nextVersion,
            passwordPolicy,
            sessionLockoutPolicy,
            accessControlPolicy,
            changeDescription,
            appliedByUserId);

        // Run combined validation
        policy.Validate();

        return policy;
    }
    /// <summary>
    /// Deactivates this policy version (called when a newer version is applied).
    /// Previous policy superseded.
    /// </summary>
    public void Deactivate(Guid modifiedByUserId)
    {
        if (!IsActive)
            throw new InvalidOperationException($"Security policy v{Version} is already inactive.");

        IsActive = false;
        EffectiveToUtc = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Validate that the combined security policy is internally consistent
    /// and does not violate hard safety constraints.
    /// Throws <see cref="InvalidOperationException"/> on validation failure.
    /// </summary>
    public void Validate()
    {
        // Individual value objects already enforce their own bounds in Create().
        // Cross-cutting validation goes here:

        // If password expiry is enabled but reuse history is 0, warn-level (not blocking).
        // This is informational; not a blocking combination.

        // If SSO is the only auth method but no SSO provider is configured,
        // the enforcement layer (not domain) should handle this.

        // Ensure at least one complexity requirement if password auth is enabled
        if (AccessControlPolicy.AllowPasswordAuthentication)
        {
            bool hasAnyComplexity = PasswordPolicy.RequireUppercase
                || PasswordPolicy.RequireLowercase
                || PasswordPolicy.RequireDigit
                || PasswordPolicy.RequireSpecialCharacter;

            if (!hasAnyComplexity && PasswordPolicy.MinLength < 12)
            {
                throw new InvalidOperationException(
                    "When no complexity requirements are set, minimum password length must be at least 12 characters " +
                    "to maintain adequate security.");
            }
        }
    }
}
