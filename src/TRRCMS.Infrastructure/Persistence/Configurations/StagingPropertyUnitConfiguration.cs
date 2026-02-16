using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingPropertyUnit entity.
/// Mirrors the PropertyUnit production table in an isolated staging area.
/// Records are validated before commit to production (FSD FR-D-4).
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingPropertyUnitConfiguration : IEntityTypeConfiguration<StagingPropertyUnit>
{
    public void Configure(EntityTypeBuilder<StagingPropertyUnit> builder)
    {
        builder.ToTable("StagingPropertyUnits");

        // Primary Key
        builder.HasKey(u => u.Id);

        // ==================== STAGING METADATA (from BaseStagingEntity) ====================

        builder.Property(u => u.ImportPackageId)
            .IsRequired();

        builder.Property(u => u.OriginalEntityId)
            .IsRequired();

        builder.Property(u => u.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(u => u.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of blocking validation error messages");

        builder.Property(u => u.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of non-blocking validation warning messages");

        builder.Property(u => u.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.CommittedEntityId);

        builder.Property(u => u.StagedAtUtc)
            .IsRequired();

        // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

        builder.Property(u => u.OriginalBuildingId)
            .IsRequired()
            .HasComment("Original Building UUID from .uhc — not a FK to production Buildings");

        // ==================== UNIT IDENTIFICATION ====================

        builder.Property(u => u.UnitIdentifier)
            .IsRequired()
            .HasMaxLength(50);

        // ==================== UNIT ATTRIBUTES ====================

        builder.Property(u => u.UnitType)
            .IsRequired();

        builder.Property(u => u.Status)
            .IsRequired();

        builder.Property(u => u.FloorNumber)
            .HasComment("Floor number (0=Ground, 1=First, -1=Basement)");

        builder.Property(u => u.NumberOfRooms)
            .HasComment("Number of rooms (عدد الغرف)");

        builder.Property(u => u.OccupancyStatus)
            .HasMaxLength(50);

        builder.Property(u => u.DamageLevel);

        builder.Property(u => u.AreaSquareMeters)
            .HasPrecision(10, 2);

        builder.Property(u => u.EstimatedAreaSqm)
            .HasPrecision(10, 2);

        builder.Property(u => u.PositionOnFloor)
            .HasMaxLength(50);

        // ==================== OCCUPANCY ====================

        builder.Property(u => u.OccupancyType);

        builder.Property(u => u.OccupancyNature);

        // ==================== UTILITIES ====================

        builder.Property(u => u.UtilitiesNotes)
            .HasMaxLength(1000);

        // ==================== DESCRIPTION ====================

        builder.Property(u => u.Description)
            .HasMaxLength(2000);

        builder.Property(u => u.SpecialFeatures)
            .HasMaxLength(2000);

        // ==================== CONCURRENCY ====================

        builder.Property(u => u.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(u => u.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==================== INDEXES ====================

        builder.HasIndex(u => u.ImportPackageId)
            .HasDatabaseName("IX_StagingPropertyUnits_ImportPackageId");

        builder.HasIndex(u => new { u.ImportPackageId, u.ValidationStatus })
            .HasDatabaseName("IX_StagingPropertyUnits_ImportPackageId_ValidationStatus");

        builder.HasIndex(u => new { u.ImportPackageId, u.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingPropertyUnits_ImportPackageId_OriginalEntityId");

        // For intra-batch referential integrity: find units by their parent building
        builder.HasIndex(u => new { u.ImportPackageId, u.OriginalBuildingId })
            .HasDatabaseName("IX_StagingPropertyUnits_ImportPackageId_OriginalBuildingId");
    }
}
