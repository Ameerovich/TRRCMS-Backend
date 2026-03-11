using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Street entity.
/// Configures PostGIS linestring geometry with SRID 4326.
/// </summary>
public class StreetConfiguration : IEntityTypeConfiguration<Street>
{
    public void Configure(EntityTypeBuilder<Street> builder)
    {
        builder.ToTable("Streets");

        // Primary Key
        builder.HasKey(s => s.Id);

        // ==================== IDENTIFICATION ====================

        builder.Property(s => s.Identifier)
            .IsRequired();

        // Unique index on Identifier
        builder.HasIndex(s => s.Identifier)
            .IsUnique()
            .HasDatabaseName("IX_Streets_Identifier");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(500);

        // ==================== SPATIAL DATA (PostGIS) ====================

        builder.Property(s => s.Geometry)
            .HasColumnType("geometry(LineString, 4326)");

        // GiST spatial index for ST_Intersects / bounding box queries
        builder.HasIndex(s => s.Geometry)
            .HasMethod("gist")
            .HasDatabaseName("IX_Streets_Geometry");

        // Ignore computed WKT property (not stored)
        builder.Ignore(s => s.GeometryWkt);

        // ==================== AUDIT FIELDS ====================

        builder.Property(s => s.CreatedAtUtc)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .IsRequired();

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.RowVersion)
            .IsRowVersion();
    }
}
