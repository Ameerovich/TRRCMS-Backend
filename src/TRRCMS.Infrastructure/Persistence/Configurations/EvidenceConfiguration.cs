using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
{
    public void Configure(EntityTypeBuilder<Evidence> builder)
    {
        builder.ToTable("Evidences");
        builder.HasKey(e => e.Id);

        // EvidenceType stored as int in database (enum conversion)
        builder.Property(e => e.EvidenceType)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("نوع الدليل - IdentificationDocument=1, OwnershipDeed=2, RentalContract=3, etc.");

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Document or evidence description");

        // File information
        builder.Property(e => e.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("Original filename as uploaded");

        builder.Property(e => e.FilePath)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("File path in storage system");

        builder.Property(e => e.FileSizeBytes)
            .IsRequired()
            .HasComment("File size in bytes");

        builder.Property(e => e.MimeType)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("MIME type (e.g., image/jpeg, application/pdf)");

        builder.Property(e => e.FileHash)
            .IsRequired(false)
            .HasMaxLength(64)
            .HasComment("SHA-256 hash of the file for integrity verification");

        // Metadata
        builder.Property(e => e.DocumentIssuedDate).IsRequired(false).HasComment("Date when the document was issued");
        builder.Property(e => e.DocumentExpiryDate).IsRequired(false).HasComment("Date when the document expires");
        builder.Property(e => e.IssuingAuthority).IsRequired(false).HasMaxLength(200).HasComment("Issuing authority or organization");
        builder.Property(e => e.DocumentReferenceNumber).IsRequired(false).HasMaxLength(100).HasComment("Document reference number");
        builder.Property(e => e.Notes).IsRequired(false).HasMaxLength(2000).HasComment("Additional notes about this evidence");

        // Versioning
        builder.Property(e => e.VersionNumber).IsRequired().HasDefaultValue(1).HasComment("Version number for document versioning");
        builder.Property(e => e.PreviousVersionId).IsRequired(false).HasComment("Reference to previous version");
        builder.Property(e => e.IsCurrentVersion).IsRequired().HasDefaultValue(true).HasComment("Indicates if this is the current/latest version");

        // Foreign keys
        builder.Property(e => e.PersonId).IsRequired(false).HasComment("Foreign key to Person");
        builder.Property(e => e.PersonPropertyRelationId).IsRequired(false).HasComment("Foreign key to PersonPropertyRelation");
        builder.Property(e => e.ClaimId).IsRequired(false).HasComment("Foreign key to Claim");

        // Audit fields
        builder.Property(e => e.CreatedAtUtc).IsRequired().HasComment("Creation timestamp (UTC)");
        builder.Property(e => e.CreatedBy).IsRequired().HasComment("User who created this record");
        builder.Property(e => e.LastModifiedAtUtc).IsRequired(false).HasComment("Last modification timestamp (UTC)");
        builder.Property(e => e.LastModifiedBy).IsRequired(false).HasComment("User who last modified this record");
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false).HasComment("Soft delete flag");
        builder.Property(e => e.DeletedAtUtc).IsRequired(false).HasComment("Deletion timestamp (UTC)");
        builder.Property(e => e.DeletedBy).IsRequired(false).HasComment("User who deleted this record");
        builder.Property(e => e.RowVersion).IsRowVersion().HasComment("Concurrency token");

        // Indexes
        builder.HasIndex(e => e.EvidenceType).HasDatabaseName("IX_Evidences_EvidenceType");
        builder.HasIndex(e => e.PersonId).HasDatabaseName("IX_Evidences_PersonId");
        builder.HasIndex(e => e.PersonPropertyRelationId).HasDatabaseName("IX_Evidences_PersonPropertyRelationId");
        builder.HasIndex(e => e.ClaimId).HasDatabaseName("IX_Evidences_ClaimId");
        builder.HasIndex(e => e.IsCurrentVersion).HasDatabaseName("IX_Evidences_IsCurrentVersion");
        builder.HasIndex(e => new { e.IsCurrentVersion, e.IsDeleted }).HasDatabaseName("IX_Evidences_IsCurrentVersion_IsDeleted");
        builder.HasIndex(e => e.IsDeleted).HasDatabaseName("IX_Evidences_IsDeleted");
        builder.HasIndex(e => e.DocumentExpiryDate).HasDatabaseName("IX_Evidences_DocumentExpiryDate");

        // Relationships
        builder.HasOne(e => e.Person)
            .WithMany(p => p.Evidences)
            .HasForeignKey(e => e.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PersonPropertyRelation)
            .WithMany(ppr => ppr.Evidences)
            .HasForeignKey(e => e.PersonPropertyRelationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PreviousVersion)
            .WithMany()
            .HasForeignKey(e => e.PreviousVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
