using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingPersonPropertyRelation entity.
/// Mirrors the PersonPropertyRelation production table in an isolated staging area.
/// </summary>
public class StagingPersonPropertyRelationConfiguration : IEntityTypeConfiguration<StagingPersonPropertyRelation>
{
    public void Configure(EntityTypeBuilder<StagingPersonPropertyRelation> builder)
    {
        builder.ToTable("StagingPersonPropertyRelations");

        // Primary Key
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ImportPackageId)
            .IsRequired();

        builder.Property(r => r.OriginalEntityId)
            .IsRequired();

        builder.Property(r => r.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(r => r.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of blocking validation error messages");

        builder.Property(r => r.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of non-blocking validation warning messages");

        builder.Property(r => r.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.CommittedEntityId);

        builder.Property(r => r.StagedAtUtc)
            .IsRequired();

        builder.Property(r => r.OriginalPersonId)
            .IsRequired()
            .HasComment("Original Person UUID from .uhc — not a FK to production Persons");

        builder.Property(r => r.OriginalPropertyUnitId)
            .IsRequired()
            .HasComment("Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits");

        builder.Property(r => r.RelationType)
            .IsRequired();

        builder.Property(r => r.OwnershipShare)
            .HasPrecision(5, 2);

        builder.Property(r => r.ContractDetails)
            .HasMaxLength(2000);

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(r => r.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.ImportPackageId)
            .HasDatabaseName("IX_StagingPersonPropertyRelations_ImportPackageId");

        builder.HasIndex(r => new { r.ImportPackageId, r.ValidationStatus })
            .HasDatabaseName("IX_StagingPersonPropertyRelations_ImportPackageId_ValidationStatus");

        builder.HasIndex(r => new { r.ImportPackageId, r.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingPersonPropertyRelations_ImportPackageId_OriginalEntityId");

        // For cross-entity validation: find relations by person or property unit
        builder.HasIndex(r => new { r.ImportPackageId, r.OriginalPersonId })
            .HasDatabaseName("IX_StagingPersonPropertyRelations_ImportPackageId_OriginalPersonId");

        builder.HasIndex(r => new { r.ImportPackageId, r.OriginalPropertyUnitId })
            .HasDatabaseName("IX_StagingPersonPropertyRelations_ImportPackageId_OriginalPropertyUnitId");
    }
}
