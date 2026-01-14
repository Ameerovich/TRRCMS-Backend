using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions");

        // Primary Key
        builder.HasKey(up => up.Id);

        // ==================== FOREIGN KEYS ====================

        // UserId - REQUIRED and INDEXED
        builder.Property(up => up.UserId)
            .IsRequired();

        builder.HasIndex(up => up.UserId);

        // ==================== PERMISSION ====================

        // Store Permission as int (enum value)
        builder.Property(up => up.Permission)
            .IsRequired()
            .HasConversion<int>();

        // Index on Permission for filtering
        builder.HasIndex(up => up.Permission);

        // ==================== COMPOSITE UNIQUE INDEX ====================

        // Ensure one user can only have ONE active grant of each permission
        // (User + Permission + IsActive) must be unique
        builder.HasIndex(up => new { up.UserId, up.Permission, up.IsActive })
     .IsUnique()
     .HasFilter("\"IsActive\" = true");

        // ==================== METADATA ====================

        builder.Property(up => up.GrantReason)
            .HasMaxLength(500);

        builder.Property(up => up.GrantedAtUtc)
            .IsRequired();

        builder.Property(up => up.GrantedBy)
            .IsRequired();

        builder.Property(up => up.ExpiresAtUtc);

        builder.Property(up => up.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ==================== AUDIT FIELDS ====================

        builder.Property(up => up.CreatedAtUtc)
            .IsRequired();

        builder.Property(up => up.CreatedBy)
            .IsRequired();

        builder.Property(up => up.LastModifiedAtUtc);

        builder.Property(up => up.LastModifiedBy);

        builder.Property(up => up.DeletedAtUtc);

        builder.Property(up => up.DeletedBy);

        builder.Property(up => up.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // ==================== RELATIONSHIPS ====================

        // Many UserPermissions belong to one User
        builder.HasOne(up => up.User)
            .WithMany(u => u.Permissions)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade); // If user is deleted, delete their permissions

        // ==================== INDEXES FOR PERFORMANCE ====================

        // Composite index for finding active permissions by user
        builder.HasIndex(up => new { up.UserId, up.IsActive });

        // Index for finding expired permissions
        builder.HasIndex(up => up.ExpiresAtUtc);

        // Index for audit queries (who granted what when)
        builder.HasIndex(up => up.GrantedBy);

        // ==================== COLUMN COMMENTS ====================

        builder.Property(up => up.UserId)
            .HasComment("Foreign key to User");

        builder.Property(up => up.Permission)
            .HasComment("Permission enum value (see Permission.cs for full list)");

        builder.Property(up => up.GrantReason)
            .HasComment("Reason why permission was granted");

        builder.Property(up => up.GrantedAtUtc)
            .HasComment("When permission was granted (UTC)");

        builder.Property(up => up.GrantedBy)
            .HasComment("User ID who granted this permission");

        builder.Property(up => up.ExpiresAtUtc)
            .HasComment("When permission expires (null = never expires)");

        builder.Property(up => up.IsActive)
            .HasComment("Whether this permission is currently active");
    }
}