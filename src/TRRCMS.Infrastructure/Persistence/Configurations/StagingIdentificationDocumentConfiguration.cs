using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingIdentificationDocument entity.
/// </summary>
public class StagingIdentificationDocumentConfiguration : IEntityTypeConfiguration<StagingIdentificationDocument>
{
    public void Configure(EntityTypeBuilder<StagingIdentificationDocument> builder)
    {
        builder.ToTable("StagingIdentificationDocuments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ImportPackageId).IsRequired();
        builder.Property(e => e.OriginalEntityId).IsRequired();

        builder.Property(e => e.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(e => e.ValidationErrors).HasMaxLength(8000);
        builder.Property(e => e.ValidationWarnings).HasMaxLength(8000);
        builder.Property(e => e.IsApprovedForCommit).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.CommittedEntityId);
        builder.Property(e => e.StagedAtUtc).IsRequired();

        builder.Property(e => e.OriginalPersonId)
            .IsRequired()
            .HasComment("Original Person UUID from .uhc");

        builder.Property(e => e.DocumentType).IsRequired();

        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.FileSizeBytes).IsRequired();
        builder.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.FileHash).HasMaxLength(128);

        builder.Property(e => e.IssuingAuthority).HasMaxLength(200);
        builder.Property(e => e.DocumentReferenceNumber).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.Property(e => e.RowVersion).IsRowVersion();

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(e => e.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ImportPackageId)
            .HasDatabaseName("IX_StagingIdentificationDocuments_ImportPackageId");

        builder.HasIndex(e => new { e.ImportPackageId, e.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingIdentificationDocuments_ImportPackageId_OriginalEntityId");

        builder.HasIndex(e => new { e.ImportPackageId, e.OriginalPersonId })
            .HasDatabaseName("IX_StagingIdentificationDocuments_ImportPackageId_OriginalPersonId");

        builder.HasIndex(e => e.FileHash)
            .HasDatabaseName("IX_StagingIdentificationDocuments_FileHash");
    }
}
