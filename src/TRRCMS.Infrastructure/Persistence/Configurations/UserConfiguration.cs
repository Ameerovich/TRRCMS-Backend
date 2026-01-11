using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Primary Key
        builder.HasKey(u => u.Id);

        // ==================== UNIQUE CONSTRAINTS ====================

        // Username - UNIQUE and INDEXED
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.Username)
            .IsUnique();

        // Email - UNIQUE (nullable) and INDEXED
        builder.Property(u => u.Email)
            .HasMaxLength(100);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // ==================== AUTHENTICATION ====================

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordSalt)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.SecurityStamp)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        builder.Property(u => u.RefreshTokenExpiryDate);

        // ==================== PERSONAL INFORMATION ====================

        builder.Property(u => u.FullNameArabic)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.FullNameEnglish)
            .HasMaxLength(200);

        builder.Property(u => u.EmployeeId)
            .HasMaxLength(50);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.Organization)
            .HasMaxLength(200);

        builder.Property(u => u.JobTitle)
            .HasMaxLength(100);

        // ==================== ROLE & PERMISSIONS ====================

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>(); // Store enum as int

        builder.Property(u => u.AdditionalRoles)
            .HasMaxLength(500);

        builder.Property(u => u.HasMobileAccess)
            .IsRequired();

        builder.Property(u => u.HasDesktopAccess)
            .IsRequired();

        // ==================== ACCOUNT STATUS ====================

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.IsLockedOut)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.LockoutEndDate);

        builder.Property(u => u.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.LastFailedLoginDate);

        // ==================== LOGIN TRACKING ====================

        builder.Property(u => u.LastLoginDate);

        builder.Property(u => u.LastPasswordChangeDate);

        builder.Property(u => u.MustChangePassword)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.PasswordExpiryDate);

        // ==================== TABLET ASSIGNMENT ====================

        builder.Property(u => u.AssignedTabletId)
            .HasMaxLength(50);

        builder.Property(u => u.TabletAssignedDate);

        // ==================== SUPERVISION ====================

        builder.Property(u => u.SupervisorUserId);

        builder.Property(u => u.TeamName)
            .HasMaxLength(100);

        // ==================== PREFERENCES ====================

        builder.Property(u => u.PreferredLanguage)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("ar");

        builder.Property(u => u.Preferences)
            .HasMaxLength(2000);

        // ==================== SECURITY ====================

        builder.Property(u => u.TwoFactorEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        // ==================== NOTES ====================

        builder.Property(u => u.Notes)
            .HasMaxLength(1000);

        // ==================== AUDIT FIELDS (from BaseAuditableEntity) ====================

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .IsRequired();

        builder.Property(u => u.LastModifiedAtUtc);

        builder.Property(u => u.LastModifiedBy);

        builder.Property(u => u.DeletedAtUtc);

        builder.Property(u => u.DeletedBy);

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // ==================== RELATIONSHIPS ====================

        // Self-referencing relationship: Supervisor
        builder.HasOne(u => u.Supervisor)
            .WithMany(u => u.Supervisees)
            .HasForeignKey(u => u.SupervisorUserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

        // ==================== INDEXES FOR PERFORMANCE ====================

        // Index on Role for filtering users by role
        builder.HasIndex(u => u.Role);

        // Index on IsActive for filtering active users
        builder.HasIndex(u => u.IsActive);

        // Index on AssignedTabletId for looking up field collectors by tablet
        builder.HasIndex(u => u.AssignedTabletId);

        // Index on SupervisorUserId for team hierarchies
        builder.HasIndex(u => u.SupervisorUserId);

        // Composite index on IsDeleted and IsActive for common queries
        builder.HasIndex(u => new { u.IsDeleted, u.IsActive });

        // ==================== COLUMN COMMENTS ====================

        builder.Property(u => u.Username)
            .HasComment("Unique username for login");

        builder.Property(u => u.Email)
            .HasComment("Email address (optional, unique if provided)");

        builder.Property(u => u.FullNameArabic)
            .HasComment("Full name in Arabic");

        builder.Property(u => u.Role)
            .HasComment("Primary user role (1=FieldCollector, 2=FieldSupervisor, 3=OfficeClerk, 4=DataManager, 5=Analyst, 6=Administrator)");

        builder.Property(u => u.IsActive)
            .HasComment("Whether the user account is active");

        builder.Property(u => u.IsLockedOut)
            .HasComment("Whether the account is locked due to failed login attempts");

        builder.Property(u => u.AssignedTabletId)
            .HasComment("Tablet device ID assigned to this field collector");
    }
}