using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for BuildingAssignment entity
/// UC-012: Assign Buildings to Field Collectors
/// </summary>
public class BuildingAssignmentConfiguration : IEntityTypeConfiguration<BuildingAssignment>
{
    public void Configure(EntityTypeBuilder<BuildingAssignment> builder)
    {
        builder.ToTable("BuildingAssignments");

        // Primary Key
        builder.HasKey(ba => ba.Id);

        // ==================== RELATIONSHIPS ====================

        builder.Property(ba => ba.BuildingId)
            .IsRequired();

        builder.Property(ba => ba.FieldCollectorId)
            .IsRequired();

        builder.Property(ba => ba.AssignedByUserId);

        // Building relationship
        builder.HasOne(ba => ba.Building)
            .WithMany(b => b.BuildingAssignments)
            .HasForeignKey(ba => ba.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing relationship for revisit assignments
        builder.HasOne(ba => ba.OriginalAssignment)
            .WithMany()
            .HasForeignKey(ba => ba.OriginalAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== ASSIGNMENT DETAILS ====================

        builder.Property(ba => ba.AssignedDate)
            .IsRequired()
            .HasComment("Date when building was assigned");

        builder.Property(ba => ba.TargetCompletionDate)
            .HasComment("Target completion date for the assignment");

        builder.Property(ba => ba.ActualCompletionDate)
            .HasComment("Actual completion date");

        // ==================== TRANSFER STATUS ====================

        builder.Property(ba => ba.TransferStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Transfer status (1=Pending, 2=InProgress, 3=Transferred, 4=Failed, 5=Cancelled, 6=PartialTransfer, 7=Synchronized)");

        builder.Property(ba => ba.TransferredToTabletDate)
            .HasComment("Date when data was transferred to tablet");

        builder.Property(ba => ba.SynchronizedFromTabletDate)
            .HasComment("Date when data was synchronized back from tablet");

        builder.Property(ba => ba.TransferErrorMessage)
            .HasMaxLength(2000)
            .HasComment("Error message if transfer failed");

        builder.Property(ba => ba.TransferRetryCount)
            .HasDefaultValue(0)
            .HasComment("Number of transfer retry attempts");

        builder.Property(ba => ba.LastTransferAttemptDate)
            .HasComment("Last transfer attempt date");

        // ==================== REVISIT TRACKING ====================

        builder.Property(ba => ba.UnitsForRevisit)
            .HasColumnType("jsonb")
            .HasComment("JSON array of property unit IDs for revisit");

        builder.Property(ba => ba.RevisitReason)
            .HasMaxLength(1000)
            .HasComment("Reason for revisit");

        builder.Property(ba => ba.IsRevisit)
            .HasDefaultValue(false)
            .HasComment("Indicates if this is a revisit assignment");

        // ==================== ASSIGNMENT STATUS ====================

        builder.Property(ba => ba.Priority)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Normal")
            .HasComment("Assignment priority (Normal, High, Urgent)");

        builder.Property(ba => ba.AssignmentNotes)
            .HasMaxLength(2000)
            .HasComment("Assignment notes/instructions");

        builder.Property(ba => ba.IsActive)
            .HasDefaultValue(true)
            .HasComment("Indicates if assignment is currently active");

        builder.Property(ba => ba.TotalPropertyUnits)
            .HasDefaultValue(0)
            .HasComment("Total property units in the building");

        builder.Property(ba => ba.CompletedPropertyUnits)
            .HasDefaultValue(0)
            .HasComment("Number of property units surveyed");

        // ==================== INDEXES ====================

        // Index on FieldCollectorId (for getting collector's assignments)
        builder.HasIndex(ba => ba.FieldCollectorId)
            .HasDatabaseName("IX_BuildingAssignments_FieldCollectorId");

        // Index on BuildingId (for checking building assignments)
        builder.HasIndex(ba => ba.BuildingId)
            .HasDatabaseName("IX_BuildingAssignments_BuildingId");

        // Composite index for active assignments by collector
        builder.HasIndex(ba => new { ba.FieldCollectorId, ba.IsActive })
            .HasDatabaseName("IX_BuildingAssignments_FieldCollector_Active");

        // Composite index for transfer status queries
        builder.HasIndex(ba => new { ba.TransferStatus, ba.IsActive })
            .HasDatabaseName("IX_BuildingAssignments_TransferStatus_Active");

        // Index on AssignedDate for date-range queries
        builder.HasIndex(ba => ba.AssignedDate)
            .HasDatabaseName("IX_BuildingAssignments_AssignedDate");

        // Composite index for building + active status
        builder.HasIndex(ba => new { ba.BuildingId, ba.IsActive })
            .HasDatabaseName("IX_BuildingAssignments_Building_Active");

        // Index for soft delete queries
        builder.HasIndex(ba => ba.IsDeleted)
            .HasDatabaseName("IX_BuildingAssignments_IsDeleted")
            .HasFilter("\"IsDeleted\" = false");
    }
}
