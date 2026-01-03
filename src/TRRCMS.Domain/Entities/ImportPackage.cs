using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Import Package entity - tracks .uhc container packages during import workflow
/// Supports staging, validation, and conflict resolution
/// </summary>
public class ImportPackage : BaseAuditableEntity
{
    // ==================== PACKAGE IDENTIFICATION ====================

    /// <summary>
    /// Unique package identifier (from .uhc manifest)
    /// </summary>
    public Guid PackageId { get; private set; }

    /// <summary>
    /// Package number for human reference
    /// Format: PKG-YYYY-NNNN
    /// </summary>
    public string PackageNumber { get; private set; }

    /// <summary>
    /// Original filename of the .uhc container
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; private set; }

    // ==================== PACKAGE METADATA ====================

    /// <summary>
    /// Date when package was created on tablet
    /// </summary>
    public DateTime PackageCreatedDate { get; private set; }

    /// <summary>
    /// Date when package was exported from tablet
    /// </summary>
    public DateTime PackageExportedDate { get; private set; }

    /// <summary>
    /// Field collector who created the package
    /// </summary>
    public Guid ExportedByUserId { get; private set; }

    /// <summary>
    /// Tablet/device ID that created the package
    /// </summary>
    public string? DeviceId { get; private set; }

    // ==================== IMPORT STATUS ====================

    /// <summary>
    /// Current import status
    /// </summary>
    public ImportStatus Status { get; private set; }

    /// <summary>
    /// Date when package was imported to desktop system
    /// </summary>
    public DateTime? ImportedDate { get; private set; }

    /// <summary>
    /// User who imported the package
    /// </summary>
    public Guid? ImportedByUserId { get; private set; }

    /// <summary>
    /// Date when validation started
    /// </summary>
    public DateTime? ValidationStartedDate { get; private set; }

    /// <summary>
    /// Date when validation completed
    /// </summary>
    public DateTime? ValidationCompletedDate { get; private set; }

    /// <summary>
    /// Date when data was committed to database
    /// </summary>
    public DateTime? CommittedDate { get; private set; }

    /// <summary>
    /// User who committed the data
    /// </summary>
    public Guid? CommittedByUserId { get; private set; }

    // ==================== SECURITY & VALIDATION ====================

    /// <summary>
    /// SHA-256 checksum of the package file
    /// </summary>
    public string Checksum { get; private set; }

    /// <summary>
    /// Digital signature of the package (if signed)
    /// </summary>
    public string? DigitalSignature { get; private set; }

    /// <summary>
    /// Indicates if package signature is valid
    /// </summary>
    public bool IsSignatureValid { get; private set; }

    /// <summary>
    /// Indicates if checksum is valid
    /// </summary>
    public bool IsChecksumValid { get; private set; }

    // ==================== CONTENT SUMMARY ====================

    /// <summary>
    /// Number of surveys in the package
    /// </summary>
    public int SurveyCount { get; private set; }

    /// <summary>
    /// Number of buildings in the package
    /// </summary>
    public int BuildingCount { get; private set; }

    /// <summary>
    /// Number of property units in the package
    /// </summary>
    public int PropertyUnitCount { get; private set; }

    /// <summary>
    /// Number of persons in the package
    /// </summary>
    public int PersonCount { get; private set; }

    /// <summary>
    /// Number of claims in the package
    /// </summary>
    public int ClaimCount { get; private set; }

    /// <summary>
    /// Number of documents/evidence files in the package
    /// </summary>
    public int DocumentCount { get; private set; }

    /// <summary>
    /// Total attachment size in bytes
    /// </summary>
    public long TotalAttachmentSizeBytes { get; private set; }

    // ==================== VOCABULARY COMPATIBILITY ====================

    /// <summary>
    /// Vocabulary versions used in this package (stored as JSON)
    /// Format: {"ownership_type": "1.2.0", "document_type": "2.1.3", ...}
    /// </summary>
    public string? VocabularyVersions { get; private set; }

    /// <summary>
    /// Indicates if vocabulary versions are compatible
    /// </summary>
    public bool IsVocabularyCompatible { get; private set; }

    /// <summary>
    /// Vocabulary compatibility issues (if any)
    /// </summary>
    public string? VocabularyCompatibilityIssues { get; private set; }

    // ==================== VALIDATION RESULTS ====================

    /// <summary>
    /// Schema version of the package
    /// </summary>
    public string? SchemaVersion { get; private set; }

    /// <summary>
    /// Indicates if schema is valid
    /// </summary>
    public bool IsSchemaValid { get; private set; }

    /// <summary>
    /// Validation errors (stored as JSON array)
    /// </summary>
    public string? ValidationErrors { get; private set; }

    /// <summary>
    /// Validation warnings (stored as JSON array)
    /// </summary>
    public string? ValidationWarnings { get; private set; }

    /// <summary>
    /// Number of validation errors
    /// </summary>
    public int ValidationErrorCount { get; private set; }

    /// <summary>
    /// Number of validation warnings
    /// </summary>
    public int ValidationWarningCount { get; private set; }

    // ==================== CONFLICT DETECTION ====================

    /// <summary>
    /// Number of person duplicates detected
    /// </summary>
    public int PersonDuplicateCount { get; private set; }

    /// <summary>
    /// Number of property duplicates detected
    /// </summary>
    public int PropertyDuplicateCount { get; private set; }

    /// <summary>
    /// Number of conflicts requiring human review
    /// </summary>
    public int ConflictCount { get; private set; }

    /// <summary>
    /// Indicates if conflicts have been resolved
    /// </summary>
    public bool AreConflictsResolved { get; private set; }

    // ==================== IMPORT RESULTS ====================

    /// <summary>
    /// Number of records successfully imported
    /// </summary>
    public int SuccessfulImportCount { get; private set; }

    /// <summary>
    /// Number of records that failed to import
    /// </summary>
    public int FailedImportCount { get; private set; }

    /// <summary>
    /// Number of records skipped (duplicates, etc.)
    /// </summary>
    public int SkippedRecordCount { get; private set; }

    /// <summary>
    /// Import summary/notes
    /// </summary>
    public string? ImportSummary { get; private set; }

    // ==================== ERROR TRACKING ====================

    /// <summary>
    /// Error message if import failed
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Detailed error log (stored as JSON)
    /// </summary>
    public string? ErrorLog { get; private set; }

    // ==================== ARCHIVAL ====================

    /// <summary>
    /// File path where original .uhc package is archived
    /// Per FSD: archives/YYYY/MM/[package_id].uhc
    /// </summary>
    public string? ArchivePath { get; private set; }

    /// <summary>
    /// Indicates if package is archived (immutable store)
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Date when package was archived
    /// </summary>
    public DateTime? ArchivedDate { get; private set; }

    // ==================== PROCESSING METADATA ====================

    /// <summary>
    /// Processing notes from data manager
    /// </summary>
    public string? ProcessingNotes { get; private set; }

    /// <summary>
    /// Import method (Manual, NetworkSync, WatchedFolder)
    /// </summary>
    public string? ImportMethod { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    // Note: ExportedByUser, ImportedByUser, CommittedByUser would be User entities
    // public virtual User ExportedByUser { get; private set; } = null!;
    // public virtual User? ImportedByUser { get; private set; }
    // public virtual User? CommittedByUser { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private ImportPackage() : base()
    {
        PackageNumber = string.Empty;
        FileName = string.Empty;
        Checksum = string.Empty;
        Status = ImportStatus.Pending;
        IsSignatureValid = false;
        IsChecksumValid = false;
        IsVocabularyCompatible = true;
        IsSchemaValid = false;
        ValidationErrorCount = 0;
        ValidationWarningCount = 0;
        PersonDuplicateCount = 0;
        PropertyDuplicateCount = 0;
        ConflictCount = 0;
        AreConflictsResolved = false;
        SuccessfulImportCount = 0;
        FailedImportCount = 0;
        SkippedRecordCount = 0;
        IsArchived = false;
    }

    /// <summary>
    /// Create new import package
    /// </summary>
    public static ImportPackage Create(
        Guid packageId,
        string fileName,
        long fileSizeBytes,
        string checksum,
        DateTime packageCreatedDate,
        DateTime packageExportedDate,
        Guid exportedByUserId,
        string? deviceId,
        int surveyCount,
        int buildingCount,
        int propertyUnitCount,
        int personCount,
        int claimCount,
        int documentCount,
        long totalAttachmentSizeBytes,
        Guid createdByUserId)
    {
        var package = new ImportPackage
        {
            PackageId = packageId,
            FileName = fileName,
            FileSizeBytes = fileSizeBytes,
            Checksum = checksum,
            PackageCreatedDate = packageCreatedDate,
            PackageExportedDate = packageExportedDate,
            ExportedByUserId = exportedByUserId,
            DeviceId = deviceId,
            SurveyCount = surveyCount,
            BuildingCount = buildingCount,
            PropertyUnitCount = propertyUnitCount,
            PersonCount = personCount,
            ClaimCount = claimCount,
            DocumentCount = documentCount,
            TotalAttachmentSizeBytes = totalAttachmentSizeBytes,
            Status = ImportStatus.Pending,
            IsSignatureValid = false,
            IsChecksumValid = false,
            IsVocabularyCompatible = true,
            IsSchemaValid = false,
            ValidationErrorCount = 0,
            ValidationWarningCount = 0,
            AreConflictsResolved = false,
            IsArchived = false
        };

        package.PackageNumber = GeneratePackageNumber();
        package.MarkAsCreated(createdByUserId);

        return package;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Mark package as imported
    /// </summary>
    public void MarkAsImported(Guid importedByUserId, string importMethod, Guid modifiedByUserId)
    {
        Status = ImportStatus.Validating;
        ImportedDate = DateTime.UtcNow;
        ImportedByUserId = importedByUserId;
        ImportMethod = importMethod;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set security validation results
    /// </summary>
    public void SetSecurityValidation(
        bool isChecksumValid,
        bool isSignatureValid,
        string? digitalSignature,
        Guid modifiedByUserId)
    {
        IsChecksumValid = isChecksumValid;
        IsSignatureValid = isSignatureValid;
        DigitalSignature = digitalSignature;

        if (!isChecksumValid || !isSignatureValid)
        {
            Status = ImportStatus.ValidationFailed;
            ErrorMessage = !isChecksumValid
                ? "Checksum validation failed"
                : "Digital signature validation failed";
        }

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set schema validation results
    /// </summary>
    public void SetSchemaValidation(
        bool isValid,
        string? schemaVersion,
        Guid modifiedByUserId)
    {
        IsSchemaValid = isValid;
        SchemaVersion = schemaVersion;

        if (!isValid)
        {
            Status = ImportStatus.ValidationFailed;
        }

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set vocabulary compatibility results
    /// </summary>
    public void SetVocabularyCompatibility(
        bool isCompatible,
        string? vocabularyVersionsJson,
        string? compatibilityIssues,
        Guid modifiedByUserId)
    {
        IsVocabularyCompatible = isCompatible;
        VocabularyVersions = vocabularyVersionsJson;
        VocabularyCompatibilityIssues = compatibilityIssues;

        if (!isCompatible)
        {
            Status = ImportStatus.Quarantined;
        }

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Add validation results
    /// </summary>
    public void AddValidationResults(
        string? errorsJson,
        string? warningsJson,
        int errorCount,
        int warningCount,
        Guid modifiedByUserId)
    {
        ValidationErrors = errorsJson;
        ValidationWarnings = warningsJson;
        ValidationErrorCount = errorCount;
        ValidationWarningCount = warningCount;
        ValidationCompletedDate = DateTime.UtcNow;

        if (errorCount > 0)
        {
            Status = ImportStatus.ValidationFailed;
        }
        else
        {
            Status = ImportStatus.Staging;
        }

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set conflict detection results
    /// </summary>
    public void SetConflictResults(
        int personDuplicates,
        int propertyDuplicates,
        int totalConflicts,
        Guid modifiedByUserId)
    {
        PersonDuplicateCount = personDuplicates;
        PropertyDuplicateCount = propertyDuplicates;
        ConflictCount = totalConflicts;

        if (totalConflicts > 0)
        {
            Status = ImportStatus.ReviewingConflicts;
            AreConflictsResolved = false;
        }
        else
        {
            AreConflictsResolved = true;
            Status = ImportStatus.ReadyToCommit;
        }

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark conflicts as resolved
    /// </summary>
    public void MarkConflictsResolved(Guid modifiedByUserId)
    {
        AreConflictsResolved = true;
        Status = ImportStatus.ReadyToCommit;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Start committing data to database
    /// </summary>
    public void StartCommit(Guid modifiedByUserId)
    {
        Status = ImportStatus.Committing;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark import as completed successfully
    /// </summary>
    public void MarkAsCompleted(
        int successCount,
        int failedCount,
        int skippedCount,
        string? importSummary,
        Guid committedByUserId,
        Guid modifiedByUserId)
    {
        Status = ImportStatus.Completed;
        SuccessfulImportCount = successCount;
        FailedImportCount = failedCount;
        SkippedRecordCount = skippedCount;
        ImportSummary = importSummary;
        CommittedDate = DateTime.UtcNow;
        CommittedByUserId = committedByUserId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark import as partially completed
    /// </summary>
    public void MarkAsPartiallyCompleted(
        int successCount,
        int failedCount,
        int skippedCount,
        string? importSummary,
        Guid modifiedByUserId)
    {
        Status = ImportStatus.PartiallyCompleted;
        SuccessfulImportCount = successCount;
        FailedImportCount = failedCount;
        SkippedRecordCount = skippedCount;
        ImportSummary = importSummary;
        CommittedDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark import as failed
    /// </summary>
    public void MarkAsFailed(string errorMessage, string? errorLogJson, Guid modifiedByUserId)
    {
        Status = ImportStatus.Failed;
        ErrorMessage = errorMessage;
        ErrorLog = errorLogJson;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Quarantine package
    /// </summary>
    public void Quarantine(string reason, Guid modifiedByUserId)
    {
        Status = ImportStatus.Quarantined;
        ErrorMessage = reason;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Cancel import
    /// </summary>
    public void Cancel(string cancellationReason, Guid modifiedByUserId)
    {
        Status = ImportStatus.Cancelled;
        ProcessingNotes = string.IsNullOrWhiteSpace(ProcessingNotes)
            ? $"[Cancelled]: {cancellationReason}"
            : $"{ProcessingNotes}\n[Cancelled]: {cancellationReason}";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Archive package to immutable store
    /// </summary>
    public void Archive(string archivePath, Guid modifiedByUserId)
    {
        ArchivePath = archivePath;
        IsArchived = true;
        ArchivedDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Add processing notes
    /// </summary>
    public void AddProcessingNotes(string notes, Guid modifiedByUserId)
    {
        ProcessingNotes = string.IsNullOrWhiteSpace(ProcessingNotes)
            ? notes
            : $"{ProcessingNotes}\n{notes}";
        MarkAsModified(modifiedByUserId);
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Generate package number
    /// Format: PKG-YYYY-NNNN
    /// </summary>
    private static string GeneratePackageNumber()
    {
        var year = DateTime.UtcNow.Year;
        var random = new Random();
        var sequence = random.Next(1000, 9999);
        return $"PKG-{year}-{sequence:D4}";
    }

    /// <summary>
    /// Calculate import success rate percentage
    /// </summary>
    public decimal GetSuccessRate()
    {
        var totalRecords = SuccessfulImportCount + FailedImportCount + SkippedRecordCount;
        if (totalRecords == 0)
            return 0;

        return (decimal)SuccessfulImportCount / totalRecords * 100;
    }
}