using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingEvidenceRelation entity.
/// Junction table for many-to-many evidence-to-relation links from .uhc packages.
/// </summary>
public class StagingEvidenceRelationConfiguration : IEntityTypeConfiguration<StagingEvidenceRelation>
{
    public void Configure(EntityTypeBuilder<StagingEvidenceRelation> builder)
    {
        builder.ToTable("StagingEvidenceRelations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ImportPackageId)
            .IsRequired();

        builder.Property(e => e.OriginalEntityId)
            .IsRequired();

        builder.Property(e => e.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(e => e.ValidationErrors)
            .HasMaxLength(8000);

        builder.Property(e => e.ValidationWarnings)
            .HasMaxLength(8000);

        builder.Property(e => e.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.CommittedEntityId);

        builder.Property(e => e.StagedAtUtc)
            .IsRequired();

        builder.Property(e => e.OriginalEvidenceId)
            .IsRequired()
            .HasComment("Original Evidence UUID from .uhc");

        builder.Property(e => e.OriginalPersonPropertyRelationId)
            .IsRequired()
            .HasComment("Original PersonPropertyRelation UUID from .uhc");

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(e => e.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ImportPackageId)
            .HasDatabaseName("IX_StagingEvidenceRelations_ImportPackageId");

        builder.HasIndex(e => new { e.ImportPackageId, e.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingEvidenceRelations_ImportPackageId_OriginalEntityId");

        builder.HasIndex(e => new { e.ImportPackageId, e.OriginalEvidenceId })
            .HasDatabaseName("IX_StagingEvidenceRelations_ImportPackageId_OriginalEvidenceId");
    }
}
