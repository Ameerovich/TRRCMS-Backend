using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Evidence entity
/// </summary>
public class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
{
    public void Configure(EntityTypeBuilder<Evidence> builder)
    {
        // Table name
        builder.ToTable("Evidences");

        // Primary key
        builder.HasKey(e => e.Id);

        // ==================== EVIDENCE CLASSIFICATION ====================

        builder.Property(e => e.EvidenceType)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Evidence type (controlled vocabulary)");

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Document or evidence description");

        // ==================== FILE INFORMATION ====================

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

        // ==================== METADATA ====================

        builder.Property(e => e.DocumentIssuedDate)
            .IsRequired(false)
            .HasComment("Date when the document was issued (if applicable)");

        builder.Property(e => e.DocumentExpiryDate)
            .IsRequired(false)
            .HasComment("Date when the document expires (if applicable)");

        builder.Property(e => e.IssuingAuthority)
            .IsRequired(false)
            .HasMaxLength(200)
            .HasComment("Issuing authority or organization");

        builder.Property(e => e.DocumentReferenceNumber)
            .IsRequired(false)
            .HasMaxLength(100)
            .HasComment("Document reference number (if any)");

        builder.Property(e => e.Notes)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("Additional notes about this evidence");

        // ==================== VERSIONING ====================

        builder.Property(e => e.VersionNumber)
            .IsRequired()
            .HasDefaultValue(1)
            .HasComment("Version number for document versioning");

        builder.Property(e => e.PreviousVersionId)
            .IsRequired(false)
            .HasComment("Reference to previous version (if this is an updated version)");

        builder.Property(e => e.IsCurrentVersion)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indicates if this is the current/latest version");

        // ==================== RELATIONSHIPS (Foreign Keys) ====================

        builder.Property(e => e.PersonId)
            .IsRequired(false)
            .HasComment("Foreign key to Person (if evidence is linked to a person)");

        builder.Property(e => e.PersonPropertyRelationId)
            .IsRequired(false)
            .HasComment("Foreign key to PersonPropertyRelation (if evidence supports a relation)");

        builder.Property(e => e.ClaimId)
            .IsRequired(false)
            .HasComment("Foreign key to Claim (if evidence supports a claim)");

        // ==================== AUDIT FIELDS ====================

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired()
            .HasComment("Creation timestamp (UTC)");

        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasComment("User who created this record");

        builder.Property(e => e.LastModifiedAtUtc)
            .IsRequired(false)
            .HasComment("Last modification timestamp (UTC)");

        builder.Property(e => e.LastModifiedBy)
            .IsRequired(false)
            .HasComment("User who last modified this record");

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Soft delete flag");

        builder.Property(e => e.DeletedAtUtc)
            .IsRequired(false)
            .HasComment("Deletion timestamp (UTC)");

        builder.Property(e => e.DeletedBy)
            .IsRequired(false)
            .HasComment("User who deleted this record");

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .HasComment("Concurrency token");

        // ==================== INDEXES ====================

        // Index for evidence type lookups
        builder.HasIndex(e => e.EvidenceType)
            .HasDatabaseName("IX_Evidence_EvidenceType");

        // Index for person lookups
        builder.HasIndex(e => e.PersonId)
            .HasDatabaseName("IX_Evidence_PersonId");

        // Index for person-property relation lookups
        builder.HasIndex(e => e.PersonPropertyRelationId)
            .HasDatabaseName("IX_Evidence_PersonPropertyRelationId");

        // Index for claim lookups
        builder.HasIndex(e => e.ClaimId)
            .HasDatabaseName("IX_Evidence_ClaimId");

        // Index for current version queries
        builder.HasIndex(e => e.IsCurrentVersion)
            .HasDatabaseName("IX_Evidence_IsCurrentVersion");

        // Composite index for active, non-deleted evidences
        builder.HasIndex(e => new { e.IsCurrentVersion, e.IsDeleted })
            .HasDatabaseName("IX_Evidence_IsCurrentVersion_IsDeleted");

        // Index for soft delete queries
        builder.HasIndex(e => e.IsDeleted)
            .HasDatabaseName("IX_Evidence_IsDeleted");

        // Index for document expiry date (for expiry checks)
        builder.HasIndex(e => e.DocumentExpiryDate)
            .HasDatabaseName("IX_Evidence_DocumentExpiryDate");

        // ==================== RELATIONSHIPS ====================

        // Relationship to Person (Many-to-One)
        builder.HasOne(e => e.Person)
            .WithMany(p => p.Evidences)
            .HasForeignKey(e => e.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to PersonPropertyRelation (Many-to-One)
        builder.HasOne(e => e.PersonPropertyRelation)
            .WithMany(ppr => ppr.Evidences)
            .HasForeignKey(e => e.PersonPropertyRelationId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ FIXED: Removed commented Claim relationship
        // The Claim → Evidence relationship is configured from the Claim side in ClaimConfiguration.cs
        // Configuring it from both sides causes conflicts

        // Self-referencing relationship for versioning (Previous Version)
        builder.HasOne(e => e.PreviousVersion)
            .WithMany()
            .HasForeignKey(e => e.PreviousVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
