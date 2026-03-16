using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingBuildingDocument entity.
/// Mirrors the BuildingDocument production table in an isolated staging area.
/// </summary>
public class StagingBuildingDocumentConfiguration : IEntityTypeConfiguration<StagingBuildingDocument>
{
    public void Configure(EntityTypeBuilder<StagingBuildingDocument> builder)
    {
        builder.ToTable("StagingBuildingDocuments");

        // Primary Key
        builder.HasKey(d => d.Id);

        builder.Property(d => d.ImportPackageId)
            .IsRequired();

        builder.Property(d => d.OriginalEntityId)
            .IsRequired();

        builder.Property(d => d.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(d => d.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of blocking validation error messages");

        builder.Property(d => d.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of non-blocking validation warning messages");

        builder.Property(d => d.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.CommittedEntityId);

        builder.Property(d => d.StagedAtUtc)
            .IsRequired();

        builder.Property(d => d.OriginalBuildingId)
            .IsRequired()
            .HasComment("Original Building UUID from .uhc — not a FK to production Buildings");

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(1000)
            .HasComment("File path within .uhc container or staging storage");

        builder.Property(d => d.FileSizeBytes)
            .IsRequired();

        builder.Property(d => d.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.FileHash)
            .HasMaxLength(128)
            .HasComment("SHA-256 hash for deduplication during commit");

        builder.Property(d => d.Notes)
            .HasMaxLength(2000);

        builder.Property(d => d.RowVersion)
            .IsRowVersion();

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(d => d.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.ImportPackageId)
            .HasDatabaseName("IX_StagingBuildingDocuments_ImportPackageId");

        builder.HasIndex(d => new { d.ImportPackageId, d.ValidationStatus })
            .HasDatabaseName("IX_StagingBuildingDocuments_ImportPackageId_ValidationStatus");

        builder.HasIndex(d => new { d.ImportPackageId, d.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingBuildingDocuments_ImportPackageId_OriginalEntityId");

        // SHA-256 hash for deduplication lookups
        builder.HasIndex(d => d.FileHash)
            .HasDatabaseName("IX_StagingBuildingDocuments_FileHash");

        // For cross-entity validation: find documents by parent building
        builder.HasIndex(d => new { d.ImportPackageId, d.OriginalBuildingId })
            .HasDatabaseName("IX_StagingBuildingDocuments_ImportPackageId_OriginalBuildingId");
    }
}
