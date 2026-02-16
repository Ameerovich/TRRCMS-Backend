using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingClaim entity.
/// Mirrors the Claim production table in an isolated staging area.
/// Subject to claim lifecycle validation (FR-D-4 Level 6).
/// Claims from tablets are mapped to "Submitted" lifecycle stage on commit (FR-D-2).
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingClaimConfiguration : IEntityTypeConfiguration<StagingClaim>
{
    public void Configure(EntityTypeBuilder<StagingClaim> builder)
    {
        builder.ToTable("StagingClaims");

        // Primary Key
        builder.HasKey(c => c.Id);

        // ==================== STAGING METADATA (from BaseStagingEntity) ====================

        builder.Property(c => c.ImportPackageId)
            .IsRequired();

        builder.Property(c => c.OriginalEntityId)
            .IsRequired();

        builder.Property(c => c.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(c => c.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of blocking validation error messages");

        builder.Property(c => c.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of non-blocking validation warning messages");

        builder.Property(c => c.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.CommittedEntityId);

        builder.Property(c => c.StagedAtUtc)
            .IsRequired();

        // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

        builder.Property(c => c.OriginalPropertyUnitId)
            .IsRequired()
            .HasComment("Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits");

        builder.Property(c => c.OriginalPrimaryClaimantId)
            .HasComment("Original primary claimant Person UUID from .uhc");

        // ==================== CLAIM IDENTIFICATION ====================

        builder.Property(c => c.ClaimNumber)
            .HasMaxLength(30)
            .HasComment("Optional in staging — auto-generated during commit (FR-D-8)");

        // ==================== CLAIM CLASSIFICATION ====================

        builder.Property(c => c.ClaimType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ClaimSource)
            .IsRequired();

        builder.Property(c => c.Priority)
            .IsRequired()
            .HasDefaultValue(CasePriority.Normal);

        builder.Property(c => c.LifecycleStage)
            .HasComment("Optional — auto-set to DraftPendingSubmission during commit");

        builder.Property(c => c.Status)
            .HasComment("Optional — auto-set to Draft during commit");

        // ==================== TENURE DETAILS ====================

        builder.Property(c => c.TenureContractType);

        builder.Property(c => c.OwnershipShare)
            .HasComment("Ownership percentage (0-100)");

        builder.Property(c => c.TenureStartDate)
            .HasComment("Date from which tenure/occupancy started");

        builder.Property(c => c.TenureEndDate)
            .HasComment("Date when tenure/occupancy ended");

        builder.Property(c => c.TargetCompletionDate)
            .HasComment("Target completion date for claim processing");

        // ==================== NARRATIVE ====================

        builder.Property(c => c.ClaimDescription)
            .HasMaxLength(4000);

        builder.Property(c => c.LegalBasis)
            .HasMaxLength(2000);

        builder.Property(c => c.SupportingNarrative)
            .HasMaxLength(4000);

        // ==================== EVIDENCE STATUS ====================

        builder.Property(c => c.EvidenceCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.AllRequiredDocumentsSubmitted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.MissingDocuments)
            .HasMaxLength(2000)
            .HasComment("JSON array of missing required document types");

        // ==================== VERIFICATION ====================

        builder.Property(c => c.VerificationStatus)
            .HasComment("Optional — auto-set to Pending during commit");

        builder.Property(c => c.VerificationNotes)
            .HasMaxLength(2000);

        // ==================== NOTES ====================

        builder.Property(c => c.ProcessingNotes)
            .HasMaxLength(4000);

        builder.Property(c => c.PublicRemarks)
            .HasMaxLength(2000);

        // ==================== CONCURRENCY ====================

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(c => c.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==================== INDEXES ====================

        builder.HasIndex(c => c.ImportPackageId)
            .HasDatabaseName("IX_StagingClaims_ImportPackageId");

        builder.HasIndex(c => new { c.ImportPackageId, c.ValidationStatus })
            .HasDatabaseName("IX_StagingClaims_ImportPackageId_ValidationStatus");

        builder.HasIndex(c => new { c.ImportPackageId, c.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingClaims_ImportPackageId_OriginalEntityId");

        // For cross-entity validation: find claims by property unit
        builder.HasIndex(c => new { c.ImportPackageId, c.OriginalPropertyUnitId })
            .HasDatabaseName("IX_StagingClaims_ImportPackageId_OriginalPropertyUnitId");

        // Claim number for duplicate detection
        builder.HasIndex(c => c.ClaimNumber)
            .HasDatabaseName("IX_StagingClaims_ClaimNumber");
    }
}
