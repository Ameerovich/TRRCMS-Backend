using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingHousehold entity.
/// Mirrors the Household production table in an isolated staging area.
/// </summary>
public class StagingHouseholdConfiguration : IEntityTypeConfiguration<StagingHousehold>
{
    public void Configure(EntityTypeBuilder<StagingHousehold> builder)
    {
        builder.ToTable("StagingHouseholds");

        // Primary Key
        builder.HasKey(h => h.Id);

        builder.Property(h => h.ImportPackageId)
            .IsRequired();

        builder.Property(h => h.OriginalEntityId)
            .IsRequired();

        builder.Property(h => h.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(h => h.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of blocking validation error messages");

        builder.Property(h => h.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of non-blocking validation warning messages");

        builder.Property(h => h.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(h => h.CommittedEntityId);

        builder.Property(h => h.StagedAtUtc)
            .IsRequired();

        builder.Property(h => h.OriginalPropertyUnitId)
            .IsRequired()
            .HasComment("Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits");

        builder.Property(h => h.OriginalHeadOfHouseholdPersonId)
            .HasComment("Original head-of-household Person UUID from .uhc");

        builder.Property(h => h.HeadOfHouseholdName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.HouseholdSize)
            .IsRequired();

        builder.Property(h => h.MaleCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.FemaleCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.MaleChildCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.FemaleChildCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.MaleElderlyCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.FemaleElderlyCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.MaleDisabledCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.FemaleDisabledCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.ChildCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.ElderlyCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.PersonsWithDisabilitiesCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(h => h.Notes)
            .HasMaxLength(2000);

        builder.Property(h => h.RowVersion)
            .IsRowVersion();

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(h => h.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.ImportPackageId)
            .HasDatabaseName("IX_StagingHouseholds_ImportPackageId");

        builder.HasIndex(h => new { h.ImportPackageId, h.ValidationStatus })
            .HasDatabaseName("IX_StagingHouseholds_ImportPackageId_ValidationStatus");

        builder.HasIndex(h => new { h.ImportPackageId, h.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingHouseholds_ImportPackageId_OriginalEntityId");

        // For cross-entity validation: find households by their property unit
        builder.HasIndex(h => new { h.ImportPackageId, h.OriginalPropertyUnitId })
            .HasDatabaseName("IX_StagingHouseholds_ImportPackageId_OriginalPropertyUnitId");
    }
}
