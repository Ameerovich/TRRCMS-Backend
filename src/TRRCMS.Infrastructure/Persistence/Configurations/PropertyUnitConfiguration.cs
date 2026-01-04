using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PropertyUnit entity
/// </summary>
public class PropertyUnitConfiguration : IEntityTypeConfiguration<PropertyUnit>
{
    public void Configure(EntityTypeBuilder<PropertyUnit> builder)
    {
        // Table name
        builder.ToTable("PropertyUnits");

        // Primary key
        builder.HasKey(p => p.Id);

        // ==================== IDENTIFIERS ====================

        builder.Property(p => p.BuildingId)
            .IsRequired();

        builder.Property(p => p.UnitIdentifier)
            .IsRequired()
            .HasMaxLength(50);

        // Unique constraint: One unit identifier per building
        builder.HasIndex(p => new { p.BuildingId, p.UnitIdentifier })
            .IsUnique()
            .HasDatabaseName("IX_PropertyUnits_BuildingId_UnitIdentifier");

        // ==================== UNIT ATTRIBUTES ====================

        builder.Property(p => p.FloorNumber)
            .IsRequired(false);

        builder.Property(p => p.UnitType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.DamageLevel)
            .IsRequired(false)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.AreaSquareMeters)
            .IsRequired(false)
            .HasPrecision(10, 2);

        builder.Property(p => p.NumberOfRooms)
            .IsRequired(false);

        builder.Property(p => p.NumberOfBathrooms)
            .IsRequired(false);

        builder.Property(p => p.HasBalcony)
            .IsRequired(false);

        // ==================== OCCUPANCY INFORMATION ====================

        builder.Property(p => p.OccupancyType)
            .IsRequired(false)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.OccupancyNature)
            .IsRequired(false)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.NumberOfHouseholds)
            .IsRequired(false);

        builder.Property(p => p.TotalOccupants)
            .IsRequired(false);

        // ==================== ADDITIONAL INFORMATION ====================

        builder.Property(p => p.Description)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(p => p.SpecialFeatures)
            .IsRequired(false)
            .HasMaxLength(1000);

        // ==================== AUDIT FIELDS ====================

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .IsRequired();

        builder.Property(p => p.LastModifiedAtUtc)
            .IsRequired(false);

        builder.Property(p => p.LastModifiedBy)
            .IsRequired(false);

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.DeletedAtUtc)
            .IsRequired(false);

        builder.Property(p => p.DeletedBy)
            .IsRequired(false);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================

        // Relationship to Building (Many-to-One)
        builder.HasOne(p => p.Building)
            .WithMany(b => b.PropertyUnits)
            .HasForeignKey(p => p.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Navigation properties - ignore for now (will be configured when those entities are implemented)
        builder.Ignore(p => p.Households);
        builder.Ignore(p => p.PersonRelations);
        builder.Ignore(p => p.Claims);
        builder.Ignore(p => p.Documents);
        builder.Ignore(p => p.Surveys);
        builder.Ignore(p => p.Certificates);
    }
}