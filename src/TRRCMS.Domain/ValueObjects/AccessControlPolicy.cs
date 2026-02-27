namespace TRRCMS.Domain.ValueObjects;

/// <summary>
/// Value object encapsulating access control policy parameters.
/// UC-011 S05: Configure Access Control Policies.
/// 
/// Manages authentication methods and optional IP/network restrictions.
/// Safety constraint: at least one authentication method must be enabled.
/// </summary>
public sealed class AccessControlPolicy : IEquatable<AccessControlPolicy>
{
    /// <summary>Allow username/password authentication (default: true, cannot be sole disabled method)</summary>
    public bool AllowPasswordAuthentication { get; private set; }

    /// <summary>Allow SSO/SAML authentication if configured (default: false)</summary>
    public bool AllowSsoAuthentication { get; private set; }

    /// <summary>Allow token-based authentication for API access (default: true)</summary>
    public bool AllowTokenAuthentication { get; private set; }

    /// <summary>Enforce IP allowlist for admin access (default: false)</summary>
    public bool EnforceIpAllowlist { get; private set; }

    /// <summary>
    /// Comma-separated list of allowed IP addresses/CIDR ranges for admin access.
    /// Only enforced when <see cref="EnforceIpAllowlist"/> is true.
    /// Example: "192.168.1.0/24,10.0.0.1"
    /// </summary>
    public string? IpAllowlist { get; private set; }

    /// <summary>
    /// Comma-separated list of denied IP addresses/CIDR ranges.
    /// Applied regardless of <see cref="EnforceIpAllowlist"/>.
    /// Example: "203.0.113.50"
    /// </summary>
    public string? IpDenylist { get; private set; }

    /// <summary>Restrict access to specific environment types (default: false)</summary>
    public bool RestrictByEnvironment { get; private set; }

    /// <summary>
    /// Comma-separated list of allowed environments when <see cref="RestrictByEnvironment"/> is true.
    /// Example: "Desktop,Mobile"
    /// </summary>
    public string? AllowedEnvironments { get; private set; }

    // EF Core requires a parameterless constructor
    private AccessControlPolicy() { }

    private AccessControlPolicy(
        bool allowPasswordAuthentication,
        bool allowSsoAuthentication,
        bool allowTokenAuthentication,
        bool enforceIpAllowlist,
        string? ipAllowlist,
        string? ipDenylist,
        bool restrictByEnvironment,
        string? allowedEnvironments)
    {
        AllowPasswordAuthentication = allowPasswordAuthentication;
        AllowSsoAuthentication = allowSsoAuthentication;
        AllowTokenAuthentication = allowTokenAuthentication;
        EnforceIpAllowlist = enforceIpAllowlist;
        IpAllowlist = ipAllowlist;
        IpDenylist = ipDenylist;
        RestrictByEnvironment = restrictByEnvironment;
        AllowedEnvironments = allowedEnvironments;
    }

    /// <summary>
    /// Factory method with safety validation.
    /// Prevents configurations that would lock out all users.
    /// </summary>
    public static AccessControlPolicy Create(
        bool allowPasswordAuthentication,
        bool allowSsoAuthentication,
        bool allowTokenAuthentication,
        bool enforceIpAllowlist,
        string? ipAllowlist,
        string? ipDenylist,
        bool restrictByEnvironment,
        string? allowedEnvironments)
    {
        // UC-011 S05/S06: Ensure at least one auth method is enabled
        if (!allowPasswordAuthentication && !allowSsoAuthentication && !allowTokenAuthentication)
            throw new ArgumentException(
                "At least one authentication method must be enabled. " +
                "Disabling all methods would lock out all users.");

        // If IP allowlist is enforced, it must contain at least one entry
        if (enforceIpAllowlist && string.IsNullOrWhiteSpace(ipAllowlist))
            throw new ArgumentException(
                "IP allowlist cannot be empty when enforcement is enabled. " +
                "This would block all admin access.",
                nameof(ipAllowlist));

        // If environment restriction is on, at least one environment must be allowed
        if (restrictByEnvironment && string.IsNullOrWhiteSpace(allowedEnvironments))
            throw new ArgumentException(
                "Allowed environments cannot be empty when restriction is enabled.",
                nameof(allowedEnvironments));

        return new AccessControlPolicy(
            allowPasswordAuthentication, allowSsoAuthentication, allowTokenAuthentication,
            enforceIpAllowlist, ipAllowlist?.Trim(), ipDenylist?.Trim(),
            restrictByEnvironment, allowedEnvironments?.Trim());
    }

    /// <summary>
    /// Returns the default access control policy (password + token auth, no IP restrictions).
    /// </summary>
    public static AccessControlPolicy Default() => new(
        allowPasswordAuthentication: true,
        allowSsoAuthentication: false,
        allowTokenAuthentication: true,
        enforceIpAllowlist: false,
        ipAllowlist: null,
        ipDenylist: null,
        restrictByEnvironment: false,
        allowedEnvironments: null);

    // ==================== EQUALITY ====================

    public bool Equals(AccessControlPolicy? other)
    {
        if (other is null) return false;
        return AllowPasswordAuthentication == other.AllowPasswordAuthentication
            && AllowSsoAuthentication == other.AllowSsoAuthentication
            && AllowTokenAuthentication == other.AllowTokenAuthentication
            && EnforceIpAllowlist == other.EnforceIpAllowlist
            && IpAllowlist == other.IpAllowlist
            && IpDenylist == other.IpDenylist
            && RestrictByEnvironment == other.RestrictByEnvironment
            && AllowedEnvironments == other.AllowedEnvironments;
    }

    public override bool Equals(object? obj) => Equals(obj as AccessControlPolicy);

    public override int GetHashCode() => HashCode.Combine(
        AllowPasswordAuthentication, AllowSsoAuthentication, AllowTokenAuthentication,
        EnforceIpAllowlist, IpAllowlist, IpDenylist, RestrictByEnvironment, AllowedEnvironments);
}
