using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingBuilding entity.
/// Mirrors the Building production table in an isolated staging area.
/// Records are validated before commit to production (FSD FR-D-4).
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingBuildingConfiguration : IEntityTypeConfiguration<StagingBuilding>
{
    public void Configure(EntityTypeBuilder<StagingBuilding> builder)
    {
        builder.ToTable("StagingBuildings");

        // Primary Key
        builder.HasKey(b => b.Id);

        // ==================== STAGING METADATA (from BaseStagingEntity) ====================

        builder.Property(b => b.ImportPackageId)
            .IsRequired();

        builder.Property(b => b.OriginalEntityId)
            .IsRequired();

        builder.Property(b => b.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(b => b.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of blocking validation error messages");

        builder.Property(b => b.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of non-blocking validation warning messages");

        builder.Property(b => b.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.CommittedEntityId);

        builder.Property(b => b.StagedAtUtc)
            .IsRequired();

        // ==================== BUILDING IDENTIFICATION ====================

        builder.Property(b => b.BuildingId)
            .HasMaxLength(17)
            .HasComment("Composite 17-digit ID — optional in staging, computed from admin codes during commit");

        // ==================== ADMINISTRATIVE CODES ====================

        builder.Property(b => b.GovernorateCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(b => b.DistrictCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(b => b.SubDistrictCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(b => b.CommunityCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(b => b.NeighborhoodCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(b => b.BuildingNumber)
            .IsRequired()
            .HasMaxLength(5);

        // ==================== LOCATION NAMES ====================

        builder.Property(b => b.GovernorateName)
            .HasMaxLength(100)
            .HasComment("From lookup tables — not in mobile package");

        builder.Property(b => b.DistrictName)
            .HasMaxLength(100)
            .HasComment("From lookup tables — not in mobile package");

        builder.Property(b => b.SubDistrictName)
            .HasMaxLength(100)
            .HasComment("From lookup tables — not in mobile package");

        builder.Property(b => b.CommunityName)
            .HasMaxLength(100)
            .HasComment("From lookup tables — not in mobile package");

        builder.Property(b => b.NeighborhoodName)
            .HasMaxLength(100)
            .HasComment("From lookup tables — not in mobile package");

        // ==================== BUILDING ATTRIBUTES ====================

        builder.Property(b => b.BuildingType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.DamageLevel)
            .HasConversion<string>()
            .HasMaxLength(50);

        // ==================== UNIT COUNTS (from command — required) ====================

        builder.Property(b => b.NumberOfPropertyUnits)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.NumberOfApartments)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.NumberOfShops)
            .IsRequired()
            .HasDefaultValue(0);

        // ==================== FUTURE EXPANSION (optional) ====================

        builder.Property(b => b.NumberOfFloors)
            .HasComment("Future expansion — not in current mobile package");

        builder.Property(b => b.YearOfConstruction)
            .HasComment("Future expansion — not in current mobile package");

        // ==================== SPATIAL DATA ====================

        builder.Property(b => b.BuildingGeometryWkt)
            .HasMaxLength(8000)
            .HasComment("WKT representation — converted to PostGIS geometry on commit");

        builder.Property(b => b.Latitude)
            .HasPrecision(10, 7);

        builder.Property(b => b.Longitude)
            .HasPrecision(10, 7);

        // ==================== OPTIONAL FIELDS ====================

        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.Property(b => b.Landmark)
            .HasMaxLength(500);

        builder.Property(b => b.LocationDescription)
            .HasMaxLength(1000);

        builder.Property(b => b.Notes)
            .HasMaxLength(2000);

        // ==================== CONCURRENCY ====================

        builder.Property(b => b.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(b => b.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==================== INDEXES ====================

        // Primary query: all staging records for a given import package
        builder.HasIndex(b => b.ImportPackageId)
            .HasDatabaseName("IX_StagingBuildings_ImportPackageId");

        // Filter staging records by validation status within a package
        builder.HasIndex(b => new { b.ImportPackageId, b.ValidationStatus })
            .HasDatabaseName("IX_StagingBuildings_ImportPackageId_ValidationStatus");

        // Lookup by original entity ID (for intra-batch referential integrity)
        builder.HasIndex(b => new { b.ImportPackageId, b.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingBuildings_ImportPackageId_OriginalEntityId");

        // BuildingId for duplicate detection against production data
        builder.HasIndex(b => b.BuildingId)
            .HasDatabaseName("IX_StagingBuildings_BuildingId");
    }
}
