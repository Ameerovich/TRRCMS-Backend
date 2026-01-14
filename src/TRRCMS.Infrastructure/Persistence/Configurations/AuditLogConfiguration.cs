using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for AuditLog entity
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        // ==================== PRIMARY KEY ====================
        builder.HasKey(a => a.Id);

        // ==================== AUDIT IDENTIFICATION ====================

        builder.Property(a => a.AuditLogNumber)
            .IsRequired()
            .HasComment("Sequential audit log entry number");

        // Unique index on AuditLogNumber for fast lookups
        builder.HasIndex(a => a.AuditLogNumber)
            .IsUnique();

        builder.Property(a => a.Timestamp)
            .IsRequired()
            .HasComment("When the action occurred (UTC)");

        // Index on Timestamp for date range queries
        builder.HasIndex(a => a.Timestamp);

        // ==================== ACTION DETAILS ====================

        builder.Property(a => a.ActionType)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Type of action performed (enum)");

        // Index on ActionType for filtering
        builder.HasIndex(a => a.ActionType);

        builder.Property(a => a.ActionDescription)
            .IsRequired()
            .HasMaxLength(1000)
            .HasComment("Human-readable description of the action");

        builder.Property(a => a.ActionResult)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Result of the action (Success, Failed, Partial)");

        // ==================== USER INFORMATION ====================

        builder.Property(a => a.UserId)
            .IsRequired()
            .HasComment("User who performed the action");

        // Index on UserId for user activity queries
        builder.HasIndex(a => a.UserId);

        builder.Property(a => a.Username)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Username at the time of action");

        builder.Property(a => a.UserRole)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("User's role at the time of action");

        builder.Property(a => a.UserFullName)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Full name of user for historical record");

        // ==================== ENTITY INFORMATION ====================

        builder.Property(a => a.EntityType)
            .HasMaxLength(100)
            .HasComment("Type of entity affected (e.g., Claim, Building)");

        builder.Property(a => a.EntityId)
            .HasComment("ID of the entity affected");

        // Composite index on EntityType and EntityId for entity history queries
        builder.HasIndex(a => new { a.EntityType, a.EntityId });

        builder.Property(a => a.EntityIdentifier)
            .HasMaxLength(100)
            .HasComment("Human-readable identifier (e.g., Claim Number)");

        // ==================== CHANGE TRACKING ====================

        builder.Property(a => a.OldValues)
            .HasColumnType("jsonb")
            .HasComment("Previous state stored as JSON");

        builder.Property(a => a.NewValues)
            .HasColumnType("jsonb")
            .HasComment("New state stored as JSON");

        builder.Property(a => a.ChangedFields)
            .HasMaxLength(1000)
            .HasComment("Comma-separated list of changed fields");

        // ==================== REQUEST CONTEXT ====================

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50)
            .HasComment("IP address from which action was performed");

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500)
            .HasComment("User agent (browser/app information)");

        builder.Property(a => a.SourceApplication)
            .HasMaxLength(50)
            .HasComment("Source application (Mobile, Desktop, API)");

        builder.Property(a => a.DeviceId)
            .HasMaxLength(100)
            .HasComment("Device ID for mobile/tablet actions");

        builder.Property(a => a.SessionId)
            .HasMaxLength(100)
            .HasComment("Session ID");

        // ==================== ADDITIONAL CONTEXT ====================

        builder.Property(a => a.AdditionalData)
            .HasColumnType("jsonb")
            .HasComment("Additional contextual information as JSON");

        builder.Property(a => a.ErrorMessage)
            .HasMaxLength(2000)
            .HasComment("Error message if action failed");

        builder.Property(a => a.StackTrace)
            .HasColumnType("text")
            .HasComment("Stack trace if action failed");

        // ==================== CORRELATION ====================

        builder.Property(a => a.CorrelationId)
            .HasComment("Correlation ID to group related actions");

        // Index on CorrelationId for grouping related actions
        builder.HasIndex(a => a.CorrelationId);

        builder.Property(a => a.ParentAuditLogId)
            .HasComment("Parent audit log ID for nested actions");

        // ==================== COMPLIANCE ====================

        builder.Property(a => a.IsSecuritySensitive)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates if this is a security-sensitive action");

        // Index on IsSecuritySensitive for security audit queries
        builder.HasIndex(a => a.IsSecuritySensitive);

        builder.Property(a => a.RequiresLegalRetention)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates if this action requires legal retention");

        builder.Property(a => a.RetentionEndDate)
            .HasComment("Retention end date (10+ years for legal hold)");

        // Index on RetentionEndDate for retention policy management
        builder.HasIndex(a => a.RetentionEndDate);

        // ==================== RELATIONSHIPS ====================

        // Self-referencing relationship for parent audit logs
        builder.HasOne(a => a.ParentAuditLog)
            .WithMany()
            .HasForeignKey(a => a.ParentAuditLogId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== COMPOSITE INDEXES FOR REPORTING ====================

        // Index for user activity within date range
        builder.HasIndex(a => new { a.UserId, a.Timestamp });

        // Index for entity history within date range
        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.Timestamp });

        // Index for security audit queries
        builder.HasIndex(a => new { a.IsSecuritySensitive, a.Timestamp });

        // Index for failed actions
        builder.HasIndex(a => new { a.ActionResult, a.Timestamp });
    }
}
