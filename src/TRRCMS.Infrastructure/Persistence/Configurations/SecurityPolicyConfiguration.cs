using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the SecurityPolicy entity.
/// Maps the three value objects (PasswordPolicy, SessionLockoutPolicy, AccessControlPolicy)
/// as owned types stored in the same table using column prefixes.
/// 
/// Table: SecurityPolicies
/// Indexes: IsActive (filtered), Version (unique)
/// </summary>
public class SecurityPolicyConfiguration : IEntityTypeConfiguration<SecurityPolicy>
{
    public void Configure(EntityTypeBuilder<SecurityPolicy> builder)
    {
        builder.ToTable("SecurityPolicies");

        // Primary Key
        builder.HasKey(sp => sp.Id);

        // ==================== VERSIONING ====================

        builder.Property(sp => sp.Version)
            .IsRequired()
            .HasComment("Policy version number, auto-incremented on each apply");

        builder.HasIndex(sp => sp.Version)
            .IsUnique()
            .HasDatabaseName("IX_SecurityPolicies_Version");

        builder.Property(sp => sp.IsActive)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Whether this is the currently enforced policy");

        // Filtered index: quickly find the single active policy
        builder.HasIndex(sp => sp.IsActive)
            .HasFilter("\"IsActive\" = true")
            .IsUnique()
            .HasDatabaseName("IX_SecurityPolicies_IsActive_Filtered");

        builder.Property(sp => sp.EffectiveFromUtc)
            .IsRequired()
            .HasComment("When this policy version became effective");

        builder.Property(sp => sp.EffectiveToUtc)
            .HasComment("When this policy was superseded (null if still active)");

        // ==================== PASSWORD POLICY (Owned Value Object) ====================

        builder.OwnsOne(sp => sp.PasswordPolicy, pwd =>
        {
            pwd.Property(p => p.MinLength)
                .HasColumnName("Password_MinLength")
                .IsRequired()
                .HasDefaultValue(8)
                .HasComment("Minimum password length (8–128)");

            pwd.Property(p => p.RequireUppercase)
                .HasColumnName("Password_RequireUppercase")
                .IsRequired()
                .HasDefaultValue(true);

            pwd.Property(p => p.RequireLowercase)
                .HasColumnName("Password_RequireLowercase")
                .IsRequired()
                .HasDefaultValue(true);

            pwd.Property(p => p.RequireDigit)
                .HasColumnName("Password_RequireDigit")
                .IsRequired()
                .HasDefaultValue(true);

            pwd.Property(p => p.RequireSpecialCharacter)
                .HasColumnName("Password_RequireSpecialCharacter")
                .IsRequired()
                .HasDefaultValue(true);

            pwd.Property(p => p.ExpiryDays)
                .HasColumnName("Password_ExpiryDays")
                .IsRequired()
                .HasDefaultValue(90)
                .HasComment("Days until password expires (0 = never)");

            pwd.Property(p => p.ReuseHistory)
                .HasColumnName("Password_ReuseHistory")
                .IsRequired()
                .HasDefaultValue(5)
                .HasComment("Number of previous passwords blocked (0 = none)");
        });

        // ==================== SESSION & LOCKOUT POLICY (Owned Value Object) ====================

        builder.OwnsOne(sp => sp.SessionLockoutPolicy, sl =>
        {
            sl.Property(s => s.SessionTimeoutMinutes)
                .HasColumnName("Session_TimeoutMinutes")
                .IsRequired()
                .HasDefaultValue(30)
                .HasComment("Session inactivity timeout in minutes (5–1440)");

            sl.Property(s => s.MaxFailedLoginAttempts)
                .HasColumnName("Session_MaxFailedLoginAttempts")
                .IsRequired()
                .HasDefaultValue(5)
                .HasComment("Max failed logins before lockout (3–20)");

            sl.Property(s => s.LockoutDurationMinutes)
                .HasColumnName("Session_LockoutDurationMinutes")
                .IsRequired()
                .HasDefaultValue(15)
                .HasComment("Lockout duration in minutes (1–1440)");
        });

        // ==================== ACCESS CONTROL POLICY (Owned Value Object) ====================

        builder.OwnsOne(sp => sp.AccessControlPolicy, ac =>
        {
            ac.Property(a => a.AllowPasswordAuthentication)
                .HasColumnName("Access_AllowPasswordAuth")
                .IsRequired()
                .HasDefaultValue(true);

            ac.Property(a => a.AllowSsoAuthentication)
                .HasColumnName("Access_AllowSsoAuth")
                .IsRequired()
                .HasDefaultValue(false);

            ac.Property(a => a.AllowTokenAuthentication)
                .HasColumnName("Access_AllowTokenAuth")
                .IsRequired()
                .HasDefaultValue(true);

            ac.Property(a => a.EnforceIpAllowlist)
                .HasColumnName("Access_EnforceIpAllowlist")
                .IsRequired()
                .HasDefaultValue(false);

            ac.Property(a => a.IpAllowlist)
                .HasColumnName("Access_IpAllowlist")
                .HasMaxLength(2000)
                .HasComment("Comma-separated allowed IPs/CIDR");

            ac.Property(a => a.IpDenylist)
                .HasColumnName("Access_IpDenylist")
                .HasMaxLength(2000)
                .HasComment("Comma-separated denied IPs/CIDR");

            ac.Property(a => a.RestrictByEnvironment)
                .HasColumnName("Access_RestrictByEnvironment")
                .IsRequired()
                .HasDefaultValue(false);

            ac.Property(a => a.AllowedEnvironments)
                .HasColumnName("Access_AllowedEnvironments")
                .HasMaxLength(500)
                .HasComment("Comma-separated allowed environments");
        });

        // ==================== METADATA ====================

        builder.Property(sp => sp.ChangeDescription)
            .HasMaxLength(1000)
            .HasComment("Description of changes in this version");

        builder.Property(sp => sp.AppliedByUserId)
            .IsRequired()
            .HasComment("User who approved and applied this policy");

        // ==================== AUDIT FIELDS (from BaseAuditableEntity) ====================

        builder.Property(sp => sp.CreatedAtUtc).IsRequired();
        builder.Property(sp => sp.CreatedBy).IsRequired();
        builder.Property(sp => sp.LastModifiedAtUtc);
        builder.Property(sp => sp.LastModifiedBy);
        builder.Property(sp => sp.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(sp => sp.DeletedAtUtc);
        builder.Property(sp => sp.DeletedBy);
    }
}
