using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingClaim entity.
/// Mirrors the Claim production table in an isolated staging area.
/// </summary>
public class StagingClaimConfiguration : IEntityTypeConfiguration<StagingClaim>
{
    public void Configure(EntityTypeBuilder<StagingClaim> builder)
    {
        builder.ToTable("StagingClaims");

        // Primary Key
        builder.HasKey(c => c.Id);

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

        builder.Property(c => c.OriginalPropertyUnitId)
            .IsRequired()
            .HasComment("Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits");

        builder.Property(c => c.OriginalPrimaryClaimantId)
            .HasComment("Original primary claimant Person UUID from .uhc");

        builder.Property(c => c.OriginalOriginatingSurveyId)
            .HasComment("Original Survey UUID from .uhc — maps to production OriginatingSurveyId");

        builder.Property(c => c.ClaimNumber)
            .HasMaxLength(30)
            .HasComment("Optional in staging — auto-generated during commit");

        builder.Property(c => c.ClaimType)
            .IsRequired();

        builder.Property(c => c.ClaimSource)
            .IsRequired();

        builder.Property(c => c.CaseStatus)
            .HasComment("Optional — auto-set to Open during commit");

        builder.Property(c => c.TenureContractType);

        builder.Property(c => c.OwnershipShare)
            .HasComment("Ownership percentage (0-100)");

        builder.Property(c => c.ClaimDescription)
            .HasMaxLength(4000);

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(c => c.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

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
