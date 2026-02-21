using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class EvidenceRelationConfiguration : IEntityTypeConfiguration<EvidenceRelation>
{
    public void Configure(EntityTypeBuilder<EvidenceRelation> builder)
    {
        builder.ToTable("EvidenceRelations");
        builder.HasKey(er => er.Id);

        // ==================== FOREIGN KEYS ====================

        builder.Property(er => er.EvidenceId)
            .IsRequired()
            .HasComment("Foreign key to Evidence");

        builder.Property(er => er.PersonPropertyRelationId)
            .IsRequired()
            .HasComment("Foreign key to PersonPropertyRelation");

        // ==================== METADATA ====================

        builder.Property(er => er.LinkReason)
            .HasMaxLength(500)
            .HasComment("Reason why evidence was linked to this relation");

        builder.Property(er => er.LinkedAtUtc)
            .IsRequired()
            .HasComment("When the link was created (UTC)");

        builder.Property(er => er.LinkedBy)
            .IsRequired()
            .HasComment("User ID who created this link");

        builder.Property(er => er.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Whether this link is currently active");

        // ==================== AUDIT FIELDS ====================

        builder.Property(er => er.CreatedAtUtc).IsRequired().HasComment("Creation timestamp (UTC)");
        builder.Property(er => er.CreatedBy).IsRequired().HasComment("User who created this record");
        builder.Property(er => er.LastModifiedAtUtc).IsRequired(false).HasComment("Last modification timestamp (UTC)");
        builder.Property(er => er.LastModifiedBy).IsRequired(false).HasComment("User who last modified this record");
        builder.Property(er => er.IsDeleted).IsRequired().HasDefaultValue(false).HasComment("Soft delete flag");
        builder.Property(er => er.DeletedAtUtc).IsRequired(false).HasComment("Deletion timestamp (UTC)");
        builder.Property(er => er.DeletedBy).IsRequired(false).HasComment("User who deleted this record");
        builder.Property(er => er.RowVersion).IsRowVersion().HasComment("Concurrency token");

        // ==================== INDEXES ====================

        builder.HasIndex(er => er.EvidenceId)
            .HasDatabaseName("IX_EvidenceRelations_EvidenceId");

        builder.HasIndex(er => er.PersonPropertyRelationId)
            .HasDatabaseName("IX_EvidenceRelations_PersonPropertyRelationId");

        // Composite unique index: one evidence can link to same relation only once (when active)
        builder.HasIndex(er => new { er.EvidenceId, er.PersonPropertyRelationId, er.IsActive })
            .IsUnique()
            .HasFilter("\"IsActive\" = true AND \"IsDeleted\" = false")
            .HasDatabaseName("IX_EvidenceRelations_EvidenceId_RelationId_IsActive_Unique");

        builder.HasIndex(er => new { er.IsActive, er.IsDeleted })
            .HasDatabaseName("IX_EvidenceRelations_IsActive_IsDeleted");

        builder.HasIndex(er => er.LinkedBy)
            .HasDatabaseName("IX_EvidenceRelations_LinkedBy");

        // ==================== RELATIONSHIPS ====================

        builder.HasOne(er => er.Evidence)
            .WithMany(e => e.EvidenceRelations)
            .HasForeignKey(er => er.EvidenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(er => er.PersonPropertyRelation)
            .WithMany(ppr => ppr.EvidenceRelations)
            .HasForeignKey(er => er.PersonPropertyRelationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
