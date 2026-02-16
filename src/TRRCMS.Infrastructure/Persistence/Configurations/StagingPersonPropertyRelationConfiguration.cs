using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingPersonPropertyRelation entity.
/// Mirrors the PersonPropertyRelation production table in an isolated staging area.
/// Subject to cross-entity relation validation (FR-D-4 Level 2) and
/// ownership evidence validation (FR-D-4 Level 3).
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingPersonPropertyRelationConfiguration : IEntityTypeConfiguration<StagingPersonPropertyRelation>
{
    public void Configure(EntityTypeBuilder<StagingPersonPropertyRelation> builder)
    {
        builder.ToTable("StagingPersonPropertyRelations");

        // Primary Key
        builder.HasKey(r => r.Id);

        // ==================== STAGING METADATA (from BaseStagingEntity) ====================

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

        // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

        builder.Property(r => r.OriginalPersonId)
            .IsRequired()
            .HasComment("Original Person UUID from .uhc — not a FK to production Persons");

        builder.Property(r => r.OriginalPropertyUnitId)
            .IsRequired()
            .HasComment("Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits");

        // ==================== RELATION DETAILS ====================

        builder.Property(r => r.RelationType)
            .IsRequired();

        builder.Property(r => r.RelationTypeOtherDesc)
            .HasMaxLength(200);

        builder.Property(r => r.ContractType);

        builder.Property(r => r.ContractTypeOtherDesc)
            .HasMaxLength(200);

        builder.Property(r => r.OwnershipShare)
            .HasPrecision(5, 2);

        builder.Property(r => r.ContractDetails)
            .HasMaxLength(2000);

        builder.Property(r => r.StartDate)
            .HasComment("Start date of the relation/contract");

        builder.Property(r => r.EndDate)
            .HasComment("End date of the relation/contract");

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ==================== CONCURRENCY ====================

        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(r => r.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==================== INDEXES ====================

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
