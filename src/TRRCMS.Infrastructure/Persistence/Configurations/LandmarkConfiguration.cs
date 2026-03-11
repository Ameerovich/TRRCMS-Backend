using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Landmark entity.
/// Configures PostGIS point geometry with SRID 4326.
/// </summary>
public class LandmarkConfiguration : IEntityTypeConfiguration<Landmark>
{
    public void Configure(EntityTypeBuilder<Landmark> builder)
    {
        builder.ToTable("Landmarks");

        // Primary Key
        builder.HasKey(l => l.Id);

        // ==================== IDENTIFICATION ====================

        builder.Property(l => l.Identifier)
            .IsRequired();

        // Unique index on Identifier
        builder.HasIndex(l => l.Identifier)
            .IsUnique()
            .HasDatabaseName("IX_Landmarks_Identifier");

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(500);

        // Index on Name for search queries
        builder.HasIndex(l => l.Name)
            .HasDatabaseName("IX_Landmarks_Name");

        builder.Property(l => l.Type)
            .IsRequired();

        // Index on Type for filtered queries
        builder.HasIndex(l => l.Type)
            .HasDatabaseName("IX_Landmarks_Type");

        // ==================== SPATIAL DATA (PostGIS) ====================

        builder.Property(l => l.Location)
            .HasColumnType("geometry(Point, 4326)");

        // GiST spatial index for ST_Intersects / bounding box queries
        builder.HasIndex(l => l.Location)
            .HasMethod("gist")
            .HasDatabaseName("IX_Landmarks_Location");

        builder.Property(l => l.Latitude)
            .HasPrecision(10, 7);

        builder.Property(l => l.Longitude)
            .HasPrecision(10, 7);

        // ==================== AUDIT FIELDS ====================

        builder.Property(l => l.CreatedAtUtc)
            .IsRequired();

        builder.Property(l => l.CreatedBy)
            .IsRequired();

        builder.Property(l => l.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.RowVersion)
            .IsRowVersion();
    }
}
