using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ImportPackage entity.
/// Defines table mapping, indexes, max lengths, and column comments
/// for the .uhc import pipeline tracking table.
/// 
/// Key indexes:
/// - Unique on PackageId (idempotency: prevents duplicate import of same .uhc)
/// - Status (filter packages by workflow stage)
/// - ImportedDate (chronological queries)
/// - ExportedByUserId (filter by field collector)
/// 
/// Referenced in UC-003 and FSD FR-D-2 through FR-D-4.
/// </summary>
public class ImportPackageConfiguration : IEntityTypeConfiguration<ImportPackage>
{
    public void Configure(EntityTypeBuilder<ImportPackage> builder)
    {
        builder.ToTable("ImportPackages");

        // Primary Key
        builder.HasKey(p => p.Id);

        // ==================== PACKAGE IDENTIFICATION ====================

        builder.Property(p => p.PackageId)
            .IsRequired()
            .HasComment("Unique package identifier from .uhc manifest — enforces idempotent imports");

        builder.Property(p => p.PackageNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("Human-readable package number (PKG-YYYY-NNNN)");

        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Original filename of the .uhc container");

        builder.Property(p => p.FileSizeBytes)
            .IsRequired();

        // ==================== PACKAGE METADATA ====================

        builder.Property(p => p.PackageCreatedDate)
            .IsRequired()
            .HasComment("Date when package was created on tablet");

        builder.Property(p => p.PackageExportedDate)
            .IsRequired()
            .HasComment("Date when package was exported from tablet");

        builder.Property(p => p.ExportedByUserId)
            .IsRequired()
            .HasComment("Field collector who created the package");

        builder.Property(p => p.DeviceId)
            .HasMaxLength(100)
            .HasComment("Tablet/device ID that created the package");

        // ==================== IMPORT STATUS ====================

        builder.Property(p => p.Status)
            .IsRequired()
            .HasComment("Current import workflow status - stored as integer");

        builder.Property(p => p.ImportedDate)
            .HasComment("Date when package was uploaded to desktop system");

        builder.Property(p => p.ImportedByUserId);

        builder.Property(p => p.ValidationStartedDate);

        builder.Property(p => p.ValidationCompletedDate);

        builder.Property(p => p.CommittedDate)
            .HasComment("Date when data was committed to production tables");

        builder.Property(p => p.CommittedByUserId);

        // ==================== SECURITY & VALIDATION ====================

        builder.Property(p => p.Checksum)
            .IsRequired()
            .HasMaxLength(128)
            .HasComment("SHA-256 checksum of the .uhc file");

        builder.Property(p => p.DigitalSignature)
            .HasMaxLength(2048)
            .HasComment("Digital signature of the package (if signed)");

        builder.Property(p => p.IsSignatureValid)
            .IsRequired();

        builder.Property(p => p.IsChecksumValid)
            .IsRequired();

        // ==================== CONTENT SUMMARY ====================

        builder.Property(p => p.SurveyCount).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.BuildingCount).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.PropertyUnitCount).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.PersonCount).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.ClaimCount).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.DocumentCount).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.TotalAttachmentSizeBytes).IsRequired().HasDefaultValue(0L);

        // ==================== VOCABULARY COMPATIBILITY ====================

        builder.Property(p => p.VocabularyVersions)
            .HasMaxLength(4000)
            .HasComment("JSON object of vocabulary versions: {\"ownership_type\": \"1.2.0\", ...}");

        builder.Property(p => p.IsVocabularyCompatible)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.VocabularyCompatibilityIssues)
            .HasMaxLength(4000)
            .HasComment("Vocabulary compatibility issues (if any)");

        // ==================== VALIDATION RESULTS ====================

        builder.Property(p => p.SchemaVersion)
            .HasMaxLength(20);

        builder.Property(p => p.IsSchemaValid)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of validation error messages");

        builder.Property(p => p.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of validation warning messages");

        builder.Property(p => p.ValidationErrorCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ValidationWarningCount)
            .IsRequired()
            .HasDefaultValue(0);

        // ==================== CONFLICT DETECTION ====================

        builder.Property(p => p.PersonDuplicateCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.PropertyDuplicateCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ConflictCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.AreConflictsResolved)
            .IsRequired()
            .HasDefaultValue(false);

        // ==================== IMPORT RESULTS ====================

        builder.Property(p => p.SuccessfulImportCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.FailedImportCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.SkippedRecordCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.ImportSummary)
            .HasMaxLength(4000);

        // ==================== ERROR TRACKING ====================

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(p => p.ErrorLog)
            .HasComment("Detailed error log (stored as JSON)");

        // ==================== ARCHIVAL ====================

        builder.Property(p => p.ArchivePath)
            .HasMaxLength(1000)
            .HasComment("File path: archives/YYYY/MM/[package_id].uhc");

        builder.Property(p => p.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.ArchivedDate);

        // ==================== PROCESSING METADATA ====================

        builder.Property(p => p.ProcessingNotes)
            .HasMaxLength(4000);

        builder.Property(p => p.ImportMethod)
            .HasMaxLength(50)
            .HasComment("Import method: Manual, NetworkSync, WatchedFolder");

        // ==================== AUDIT FIELDS (from BaseAuditableEntity) ====================

        builder.Property(p => p.CreatedAtUtc).IsRequired();
        builder.Property(p => p.CreatedBy).IsRequired();
        builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);

        // ==================== CONCURRENCY ====================

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // ==================== INDEXES ====================

        // Idempotency: prevent duplicate import of same .uhc package
        builder.HasIndex(p => p.PackageId)
            .IsUnique()
            .HasDatabaseName("IX_ImportPackages_PackageId");

        // Filter by workflow status (most common query pattern)
        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_ImportPackages_Status");

        // Chronological queries: "show imports from last week"
        builder.HasIndex(p => p.ImportedDate)
            .HasDatabaseName("IX_ImportPackages_ImportedDate");

        // Filter by field collector
        builder.HasIndex(p => p.ExportedByUserId)
            .HasDatabaseName("IX_ImportPackages_ExportedByUserId");
    }
}
