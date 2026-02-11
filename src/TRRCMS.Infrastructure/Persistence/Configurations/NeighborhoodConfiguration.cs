using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Neighborhood reference entity.
/// Configures PostGIS geometry columns with SRID 4326.
/// </summary>
public class NeighborhoodConfiguration : IEntityTypeConfiguration<Neighborhood>
{
    public void Configure(EntityTypeBuilder<Neighborhood> builder)
    {
        builder.ToTable("Neighborhoods");

        // Primary Key
        builder.HasKey(n => n.Id);

        // ==================== ADMINISTRATIVE CODES ====================

        builder.Property(n => n.GovernorateCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(n => n.DistrictCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(n => n.SubDistrictCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(n => n.CommunityCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(n => n.NeighborhoodCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(n => n.FullCode)
            .IsRequired()
            .HasMaxLength(12);

        // Unique index on FullCode (one row per neighborhood in hierarchy)
        builder.HasIndex(n => n.FullCode)
            .IsUnique()
            .HasDatabaseName("IX_Neighborhoods_FullCode");

        // Composite index for hierarchy lookups
        builder.HasIndex(n => new { n.GovernorateCode, n.DistrictCode, n.SubDistrictCode, n.CommunityCode })
            .HasDatabaseName("IX_Neighborhoods_AdminHierarchy");

        // ==================== NAMES ====================

        builder.Property(n => n.NameArabic)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.NameEnglish)
            .HasMaxLength(200);

        // ==================== SPATIAL DATA (PostGIS) ====================

        builder.Property(n => n.CenterPoint)
            .HasColumnType("geometry(Point, 4326)");

        builder.Property(n => n.CenterLatitude)
            .HasPrecision(10, 7);

        builder.Property(n => n.CenterLongitude)
            .HasPrecision(10, 7);

        builder.Property(n => n.BoundaryGeometry)
            .HasColumnType("geometry(Geometry, 4326)");

        // GiST spatial index on boundary for ST_Intersects / ST_Contains queries
        builder.HasIndex(n => n.BoundaryGeometry)
            .HasMethod("gist")
            .HasDatabaseName("IX_Neighborhoods_BoundaryGeometry");

        // Ignore computed WKT property (not stored)
        builder.Ignore(n => n.BoundaryWkt);

        builder.Property(n => n.AreaSquareKm)
            .HasPrecision(10, 4);

        builder.Property(n => n.ZoomLevel)
            .IsRequired()
            .HasDefaultValue(15);

        // ==================== STATUS ====================

        builder.Property(n => n.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ==================== AUDIT FIELDS ====================

        builder.Property(n => n.CreatedAtUtc)
            .IsRequired();

        builder.Property(n => n.CreatedBy)
            .IsRequired();

        builder.Property(n => n.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.RowVersion)
            .IsRowVersion();
    }
}
