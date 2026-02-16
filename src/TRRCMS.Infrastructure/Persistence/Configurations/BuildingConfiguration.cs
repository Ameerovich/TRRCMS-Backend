using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Building entity
/// Configures PostGIS geometry column with SRID 4326
/// </summary>
public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");

        // Primary Key
        builder.HasKey(b => b.Id);

        // ==================== BUSINESS IDENTIFIER ====================
        builder.Property(b => b.BuildingId)
            .IsRequired()
            .HasMaxLength(17);

        builder.HasIndex(b => b.BuildingId)
            .IsUnique();

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

        // Composite index for admin hierarchy searches (filtered to non-deleted)
        builder.HasIndex(b => new { b.GovernorateCode, b.DistrictCode, b.SubDistrictCode, b.CommunityCode, b.NeighborhoodCode })
            .HasDatabaseName("IX_Buildings_AdminHierarchy")
            .HasFilter("\"IsDeleted\" = false");

        // ==================== LOCATION NAMES (ARABIC) ====================
        builder.Property(b => b.GovernorateName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.DistrictName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.SubDistrictName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.CommunityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.NeighborhoodName)
            .IsRequired()
            .HasMaxLength(100);

        // ==================== BUILDING ATTRIBUTES ====================
        builder.Property(b => b.BuildingType)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired();

        builder.Property(b => b.DamageLevel);

        // ==================== OPTIONAL FIELDS ====================
        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.Property(b => b.Landmark)
            .HasMaxLength(500);

        builder.Property(b => b.LocationDescription)
            .HasMaxLength(1000);

        builder.Property(b => b.Notes)
            .HasMaxLength(2000);

        // ==================== SPATIAL DATA (PostGIS) ====================
        // Geometry column using PostGIS native type
        // SRID 4326 = WGS84 (GPS coordinate system)
        builder.Property(b => b.BuildingGeometry)
            .HasColumnType("geometry(Geometry, 4326)");

        // GiST spatial index on BuildingGeometry — critical for ST_Intersects, ST_DWithin, ST_Within
        // Without this, every spatial query does a full table scan
        builder.HasIndex(b => b.BuildingGeometry)
            .HasMethod("gist")
            .HasDatabaseName("IX_Buildings_BuildingGeometry");

        // Ignore the computed WKT property (not stored in database)
        builder.Ignore(b => b.BuildingGeometryWkt);

        // GPS Coordinates (kept for convenience)
        builder.Property(b => b.Latitude)
            .HasPrecision(10, 7);

        builder.Property(b => b.Longitude)
            .HasPrecision(10, 7);

        // ==================== AUDIT FIELDS ====================
        builder.Property(b => b.CreatedAtUtc)
            .IsRequired();

        builder.Property(b => b.CreatedBy)
            .IsRequired();

        builder.Property(b => b.LastModifiedAtUtc);
        builder.Property(b => b.LastModifiedBy);

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.DeletedAtUtc);
        builder.Property(b => b.DeletedBy);

        // Row version for concurrency control
        builder.Property(b => b.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================
        builder.Ignore(b => b.BuildingAssignments);
        builder.Ignore(b => b.Surveys);
    }
}
