using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings");

        // Primary Key
        builder.HasKey(b => b.Id);

        // Business Identifier - UNIQUE and INDEXED
        builder.Property(b => b.BuildingId)
            .IsRequired()
            .HasMaxLength(17);

        builder.HasIndex(b => b.BuildingId)
            .IsUnique();

        // Administrative Codes
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

        // Location Names (Arabic)
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

        // Building Attributes - Store enums as strings
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

        // Optional fields
        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.Property(b => b.Landmark)
            .HasMaxLength(500);

        builder.Property(b => b.Notes)
            .HasMaxLength(2000);

        // Spatial data (simplified for now - we'll enhance with PostGIS later)
        builder.Property(b => b.BuildingGeometryWkt)
            .HasMaxLength(5000);

        builder.Property(b => b.Latitude)
            .HasPrecision(10, 7);

        builder.Property(b => b.Longitude)
            .HasPrecision(10, 7);

        // Audit fields
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

        // Relationships - ignore for now (we'll add these when we implement related entities)
        builder.Ignore(b => b.PropertyUnits);
        builder.Ignore(b => b.BuildingAssignments);
        builder.Ignore(b => b.Surveys);
    }
}