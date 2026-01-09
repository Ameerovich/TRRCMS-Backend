using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Document entity
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        // Table name
        builder.ToTable("Documents");

        // Primary key
        builder.HasKey(d => d.Id);

        // ==================== DOCUMENT CLASSIFICATION ====================

        builder.Property(d => d.DocumentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(100)
            .HasComment("Document type from controlled vocabulary (e.g., TabuGreen, RentalContract, NationalIdCard)");

        builder.Property(d => d.DocumentNumber)
            .IsRequired(false)
            .HasMaxLength(100)
            .HasComment("Document number/reference (e.g., Tabu number, ID number, contract number)");

        builder.Property(d => d.DocumentTitle)
            .IsRequired(false)
            .HasMaxLength(500)
            .HasComment("Document title or description in Arabic");

        // ==================== ISSUANCE INFORMATION ====================

        builder.Property(d => d.IssueDate)
            .IsRequired(false)
            .HasComment("Date when document was issued");

        builder.Property(d => d.ExpiryDate)
            .IsRequired(false)
            .HasComment("Date when document expires (if applicable)");

        builder.Property(d => d.IssuingAuthority)
            .IsRequired(false)
            .HasMaxLength(200)
            .HasComment("Issuing authority/organization (e.g., Ministry of Interior, Aleppo Municipality)");

        builder.Property(d => d.IssuingPlace)
            .IsRequired(false)
            .HasMaxLength(200)
            .HasComment("Place where document was issued");

        // ==================== VERIFICATION STATUS ====================

        builder.Property(d => d.IsVerified)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates if document has been verified");

        builder.Property(d => d.VerificationStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(VerificationStatus.Pending)
            .HasComment("Verification status (Pending, Verified, Rejected, RequiresAdditionalInfo)");

        builder.Property(d => d.VerificationDate)
            .IsRequired(false)
            .HasComment("Date when document was verified");

        builder.Property(d => d.VerifiedByUserId)
            .IsRequired(false)
            .HasComment("User who verified the document");

        builder.Property(d => d.VerificationNotes)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("Verification notes or comments");

        // ==================== DOCUMENT CONTENT ====================

        builder.Property(d => d.EvidenceId)
            .IsRequired(false)
            .HasComment("Foreign key to Evidence entity (the actual file/scan)");

        builder.Property(d => d.DocumentHash)
            .IsRequired(false)
            .HasMaxLength(64)
            .HasComment("SHA-256 hash of the document for integrity verification");

        builder.Property(d => d.Notes)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("Additional notes about this document");

        // ==================== DOCUMENT RELATIONSHIPS ====================

        builder.Property(d => d.PersonId)
            .IsRequired(false)
            .HasComment("Foreign key to Person (if document belongs to a person)");

        builder.Property(d => d.PropertyUnitId)
            .IsRequired(false)
            .HasComment("Foreign key to PropertyUnit (if document relates to property)");

        builder.Property(d => d.PersonPropertyRelationId)
            .IsRequired(false)
            .HasComment("Foreign key to PersonPropertyRelation (if document proves a relation)");

        builder.Property(d => d.ClaimId)
            .IsRequired(false)
            .HasComment("Foreign key to Claim (if document supports a claim)");

        // ==================== LEGAL VALIDITY ====================

        builder.Property(d => d.IsLegallyValid)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indicates if document is legally valid");

        builder.Property(d => d.LegalValidityNotes)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("Legal validity notes (why valid or invalid)");

        builder.Property(d => d.IsOriginal)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indicates if document is original or a copy");

        builder.Property(d => d.OriginalDocumentId)
            .IsRequired(false)
            .HasComment("If copy, reference to original document (if in system)");

        // ==================== NOTARIZATION ====================

        builder.Property(d => d.IsNotarized)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates if document is notarized");

        builder.Property(d => d.NotaryOffice)
            .IsRequired(false)
            .HasMaxLength(200)
            .HasComment("Notary office name/number");

        builder.Property(d => d.NotarizationDate)
            .IsRequired(false)
            .HasComment("Notarization date");

        builder.Property(d => d.NotarizationNumber)
            .IsRequired(false)
            .HasMaxLength(100)
            .HasComment("Notarization number");

        // ==================== AUDIT FIELDS ====================

        builder.Property(d => d.CreatedAtUtc)
            .IsRequired()
            .HasComment("UTC timestamp when record was created");

        builder.Property(d => d.CreatedBy)
            .IsRequired()
            .HasComment("User ID who created this record");

        builder.Property(d => d.LastModifiedAtUtc)
            .IsRequired(false)
            .HasComment("UTC timestamp when record was last modified");

        builder.Property(d => d.LastModifiedBy)
            .IsRequired(false)
            .HasComment("User ID who last modified this record");

        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Soft delete flag");

        builder.Property(d => d.DeletedAtUtc)
            .IsRequired(false)
            .HasComment("UTC timestamp when record was soft deleted");

        builder.Property(d => d.DeletedBy)
            .IsRequired(false)
            .HasComment("User ID who soft deleted this record");

        // ==================== RELATIONSHIPS ====================

        // Evidence relationship
        builder.HasOne(d => d.Evidence)
            .WithMany()
            .HasForeignKey(d => d.EvidenceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Person relationship
        builder.HasOne(d => d.Person)
            .WithMany()
            .HasForeignKey(d => d.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // PropertyUnit relationship
        builder.HasOne(d => d.PropertyUnit)
            .WithMany(p => p.Documents)  // ✅ FIXED: Explicit navigation property
            .HasForeignKey(d => d.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // PersonPropertyRelation relationship
        builder.HasOne(d => d.PersonPropertyRelation)
            .WithMany()
            .HasForeignKey(d => d.PersonPropertyRelationId)
            .OnDelete(DeleteBehavior.Restrict);

        // ✅ FIXED: Removed commented Claim relationship
        // The Claim → Document relationship is configured from the Claim side in ClaimConfiguration.cs
        // Configuring it from both sides causes conflicts

        // Original Document relationship (self-referencing)
        builder.HasOne(d => d.OriginalDocument)
            .WithMany()
            .HasForeignKey(d => d.OriginalDocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== INDEXES ====================

        // Index on DocumentType for filtering by document type
        builder.HasIndex(d => d.DocumentType)
            .HasDatabaseName("IX_Documents_DocumentType");

        // Index on DocumentNumber for quick lookups
        builder.HasIndex(d => d.DocumentNumber)
            .HasDatabaseName("IX_Documents_DocumentNumber");

        // Index on VerificationStatus for filtering pending/verified documents
        builder.HasIndex(d => d.VerificationStatus)
            .HasDatabaseName("IX_Documents_VerificationStatus");

        // Index on IsVerified for quick filtering
        builder.HasIndex(d => d.IsVerified)
            .HasDatabaseName("IX_Documents_IsVerified");

        // Index on PersonId for getting all documents of a person
        builder.HasIndex(d => d.PersonId)
            .HasDatabaseName("IX_Documents_PersonId");

        // Index on PropertyUnitId for getting all documents of a property
        builder.HasIndex(d => d.PropertyUnitId)
            .HasDatabaseName("IX_Documents_PropertyUnitId");

        // Index on PersonPropertyRelationId
        builder.HasIndex(d => d.PersonPropertyRelationId)
            .HasDatabaseName("IX_Documents_PersonPropertyRelationId");

        // Index on ClaimId
        builder.HasIndex(d => d.ClaimId)
            .HasDatabaseName("IX_Documents_ClaimId");

        // Index on EvidenceId
        builder.HasIndex(d => d.EvidenceId)
            .HasDatabaseName("IX_Documents_EvidenceId");

        // Index on ExpiryDate for finding expiring/expired documents
        builder.HasIndex(d => d.ExpiryDate)
            .HasDatabaseName("IX_Documents_ExpiryDate");

        // Composite index for filtering by type and verification status
        builder.HasIndex(d => new { d.DocumentType, d.VerificationStatus })
            .HasDatabaseName("IX_Documents_DocumentType_VerificationStatus");

        // Index on IsDeleted for soft delete filtering
        builder.HasIndex(d => d.IsDeleted)
            .HasDatabaseName("IX_Documents_IsDeleted");

        // ==================== QUERY FILTERS ====================

        // Global query filter to exclude soft-deleted records
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
