using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// User Permission entity - many-to-many relationship between Users and Permissions
/// Represents which permissions a specific user has been granted
/// </summary>
public class UserPermission : BaseAuditableEntity
{
    // ==================== RELATIONSHIP KEYS ====================

    /// <summary>
    /// Foreign key to User
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Permission granted to the user
    /// </summary>
    public Permission Permission { get; private set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Why this permission was granted
    /// </summary>
    public string? GrantReason { get; private set; }

    /// <summary>
    /// When the permission was granted
    /// </summary>
    public DateTime GrantedAtUtc { get; private set; }

    /// <summary>
    /// User who granted this permission
    /// </summary>
    public Guid GrantedBy { get; private set; }

    /// <summary>
    /// Permission expiry date (null = no expiry)
    /// </summary>
    public DateTime? ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Whether this permission is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// User who has this permission
    /// </summary>
    public virtual User User { get; private set; } = null!;

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private UserPermission() : base()
    {
        IsActive = true;
        GrantedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Create new user permission
    /// </summary>
    public static UserPermission Create(
        Guid userId,
        Permission permission,
        Guid grantedBy,
        string? grantReason = null,
        DateTime? expiresAt = null)
    {
        var userPermission = new UserPermission
        {
            UserId = userId,
            Permission = permission,
            GrantedBy = grantedBy,
            GrantReason = grantReason,
            GrantedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expiresAt,
            IsActive = true
        };

        userPermission.MarkAsCreated(grantedBy);

        return userPermission;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Revoke this permission
    /// </summary>
    public void Revoke(Guid revokedBy, string reason)
    {
        IsActive = false;
        GrantReason = string.IsNullOrWhiteSpace(GrantReason)
            ? $"[Revoked]: {reason}"
            : $"{GrantReason}\n[Revoked]: {reason}";
        MarkAsModified(revokedBy);
    }

    /// <summary>
    /// Reactivate this permission
    /// </summary>
    public void Reactivate(Guid reactivatedBy, string reason)
    {
        IsActive = true;
        GrantReason = string.IsNullOrWhiteSpace(GrantReason)
            ? $"[Reactivated]: {reason}"
            : $"{GrantReason}\n[Reactivated]: {reason}";
        MarkAsModified(reactivatedBy);
    }

    /// <summary>
    /// Check if permission is expired
    /// </summary>
    public bool IsExpired()
    {
        if (!ExpiresAtUtc.HasValue)
            return false;

        return DateTime.UtcNow > ExpiresAtUtc.Value;
    }

    /// <summary>
    /// Check if permission is currently valid (active and not expired)
    /// </summary>
    public bool IsValid()
    {
        return IsActive && !IsExpired();
    }

    /// <summary>
    /// Extend expiry date
    /// </summary>
    public void ExtendExpiry(DateTime newExpiryDate, Guid modifiedBy)
    {
        if (newExpiryDate > (ExpiresAtUtc ?? DateTime.MinValue))
        {
            ExpiresAtUtc = newExpiryDate;
            MarkAsModified(modifiedBy);
        }
    }
}