using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

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

        builder.Property(p => p.BuildingId)
            .IsRequired();

        builder.Property(p => p.UnitIdentifier)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.UnitIdentifierAdjustedByKeepSeparate)
            .HasDefaultValue(false)
            .HasComment("True if UnitIdentifier was suffix-disambiguated at import commit for a Keep-Separate decision (awaits reconciliation)");

        builder.Property(p => p.OriginalUnitIdentifier)
            .HasMaxLength(50)
            .HasComment("UnitIdentifier as received before commit-time suffix adjustment for a Keep-Separate decision");

        // Unique constraint: one LIVE unit identifier per building.
        // Filtered on IsDeleted = false so a soft-deleted unit no longer reserves its
        // (BuildingId, UnitIdentifier) slot — mirrors IX_Person_NationalId. Without the filter a
        // soft-deleted (e.g. merged-away) unit permanently blocked re-importing the same identifier.
        builder.HasIndex(p => new { p.BuildingId, p.UnitIdentifier })
            .IsUnique()
            .HasDatabaseName("IX_PropertyUnits_BuildingId_UnitIdentifier")
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(p => p.FloorNumber)
            .IsRequired(false);

        builder.Property(p => p.UnitType)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.AreaSquareMeters)
            .IsRequired(false)
            .HasPrecision(10, 2);

        builder.Property(p => p.NumberOfRooms)
            .IsRequired(false);

        builder.Property(p => p.Description)
            .IsRequired(false)
            .HasMaxLength(2000);

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

        // Relationship to Building (Many-to-One)
        builder.HasOne(p => p.Building)
            .WithMany(b => b.PropertyUnits)
            .HasForeignKey(p => p.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Households (One-to-Many)
        builder.HasMany(p => p.Households)
            .WithOne(h => h.PropertyUnit)
            .HasForeignKey(h => h.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.PersonRelations)
            .WithOne(ppr => ppr.PropertyUnit)
            .HasForeignKey(ppr => ppr.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Claims)
            .WithOne(c => c.PropertyUnit)
            .HasForeignKey(c => c.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: Surveys relationship will be configured when needed
        builder.Ignore(p => p.Surveys);
    }
}
