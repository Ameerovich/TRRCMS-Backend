using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingEvidence entity.
/// Mirrors the Evidence production table in an isolated staging area.
/// Subject to attachment deduplication by SHA-256 hash (FSD FR-D-9).
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingEvidenceConfiguration : IEntityTypeConfiguration<StagingEvidence>
{
    public void Configure(EntityTypeBuilder<StagingEvidence> builder)
    {
        builder.ToTable("StagingEvidences");

        // Primary Key
        builder.HasKey(e => e.Id);

        // ==================== STAGING METADATA (from BaseStagingEntity) ====================

        builder.Property(e => e.ImportPackageId)
            .IsRequired();

        builder.Property(e => e.OriginalEntityId)
            .IsRequired();

        builder.Property(e => e.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(e => e.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of blocking validation error messages");

        builder.Property(e => e.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of non-blocking validation warning messages");

        builder.Property(e => e.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.CommittedEntityId);

        builder.Property(e => e.StagedAtUtc)
            .IsRequired();

        // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

        builder.Property(e => e.OriginalPersonId)
            .HasComment("Original Person UUID from .uhc — not a FK to production Persons");

        builder.Property(e => e.OriginalPersonPropertyRelationId)
            .HasComment("Original PersonPropertyRelation UUID from .uhc");

        builder.Property(e => e.OriginalClaimId)
            .HasComment("Original Claim UUID from .uhc");

        // ==================== EVIDENCE METADATA ====================

        builder.Property(e => e.EvidenceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.FilePath)
            .IsRequired()
            .HasMaxLength(1000)
            .HasComment("File path within .uhc container or staging storage");

        builder.Property(e => e.FileSizeBytes)
            .IsRequired();

        builder.Property(e => e.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FileHash)
            .HasMaxLength(128)
            .HasComment("SHA-256 hash for deduplication during commit (FR-D-9)");

        // ==================== DOCUMENT DETAILS ====================

        builder.Property(e => e.IssuingAuthority)
            .HasMaxLength(200);

        builder.Property(e => e.DocumentIssuedDate)
            .HasComment("Date when document was issued");

        builder.Property(e => e.DocumentExpiryDate)
            .HasComment("Date when document expires");

        builder.Property(e => e.DocumentReferenceNumber)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        // ==================== VERSION TRACKING ====================

        builder.Property(e => e.VersionNumber)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.OriginalPreviousVersionId)
            .HasComment("Original previous version UUID from .uhc");

        builder.Property(e => e.IsCurrentVersion)
            .IsRequired()
            .HasDefaultValue(true);

        // ==================== CONCURRENCY ====================

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(e => e.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==================== INDEXES ====================

        builder.HasIndex(e => e.ImportPackageId)
            .HasDatabaseName("IX_StagingEvidences_ImportPackageId");

        builder.HasIndex(e => new { e.ImportPackageId, e.ValidationStatus })
            .HasDatabaseName("IX_StagingEvidences_ImportPackageId_ValidationStatus");

        builder.HasIndex(e => new { e.ImportPackageId, e.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingEvidences_ImportPackageId_OriginalEntityId");

        // SHA-256 hash for deduplication lookups (FR-D-9)
        builder.HasIndex(e => e.FileHash)
            .HasDatabaseName("IX_StagingEvidences_FileHash");

        // For cross-entity validation: find evidence by parent entities
        builder.HasIndex(e => new { e.ImportPackageId, e.OriginalPersonId })
            .HasDatabaseName("IX_StagingEvidences_ImportPackageId_OriginalPersonId");

        builder.HasIndex(e => new { e.ImportPackageId, e.OriginalClaimId })
            .HasDatabaseName("IX_StagingEvidences_ImportPackageId_OriginalClaimId");
    }
}
