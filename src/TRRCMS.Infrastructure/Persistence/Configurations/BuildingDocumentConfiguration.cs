using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for BuildingDocument entity.
/// Follows the same pattern as EvidenceConfiguration.
/// </summary>
public class BuildingDocumentConfiguration : IEntityTypeConfiguration<BuildingDocument>
{
    public void Configure(EntityTypeBuilder<BuildingDocument> builder)
    {
        builder.ToTable("BuildingDocuments");
        builder.HasKey(d => d.Id);

        // ==================== RELATIONSHIP ====================

        builder.Property(d => d.BuildingId)
            .IsRequired();

        builder.HasOne<Building>()
            .WithMany(b => b.BuildingDocuments)
            .HasForeignKey(d => d.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.BuildingId)
            .HasDatabaseName("IX_BuildingDocuments_BuildingId");

        // ==================== DOCUMENT METADATA ====================

        builder.Property(d => d.Description)
            .IsRequired(false)
            .HasMaxLength(500)
            .HasComment("Optional description of the document");

        // ==================== FILE INFORMATION ====================

        builder.Property(d => d.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("Original filename as uploaded");

        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("File path in storage system");

        builder.Property(d => d.FileSizeBytes)
            .IsRequired()
            .HasComment("File size in bytes");

        builder.Property(d => d.MimeType)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("MIME type (e.g., image/jpeg, application/pdf)");

        builder.Property(d => d.FileHash)
            .IsRequired(false)
            .HasMaxLength(128)
            .HasComment("SHA-256 hash of the file for deduplication");

        builder.Property(d => d.Notes)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("Additional notes");

        // ==================== AUDIT FIELDS ====================

        builder.Property(d => d.CreatedAtUtc).IsRequired();
        builder.Property(d => d.CreatedBy).IsRequired();
        builder.Property(d => d.LastModifiedAtUtc);
        builder.Property(d => d.LastModifiedBy);
        builder.Property(d => d.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(d => d.DeletedAtUtc);
        builder.Property(d => d.DeletedBy);
        builder.Property(d => d.RowVersion).IsRowVersion();

        // ==================== INDEXES ====================

        builder.HasIndex(d => d.FileHash)
            .HasDatabaseName("IX_BuildingDocuments_FileHash");

        builder.HasIndex(d => d.IsDeleted)
            .HasDatabaseName("IX_BuildingDocuments_IsDeleted");
    }
}
