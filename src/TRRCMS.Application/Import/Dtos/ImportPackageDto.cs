namespace TRRCMS.Application.Import.Dtos;

/// <summary>
/// Data Transfer Object for ImportPackage entity.
/// Used as the API response for upload, status queries, and package listing.
/// </summary>
public class ImportPackageDto
{
    // ==================== IDENTIFICATION ====================

    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public string PackageNumber { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    // ==================== METADATA ====================

    public DateTime PackageCreatedDate { get; set; }
    public DateTime PackageExportedDate { get; set; }
    public Guid ExportedByUserId { get; set; }
    public string? DeviceId { get; set; }

    // ==================== STATUS ====================

    public string Status { get; set; } = string.Empty;
    public DateTime? ImportedDate { get; set; }
    public Guid? ImportedByUserId { get; set; }
    public DateTime? ValidationStartedDate { get; set; }
    public DateTime? ValidationCompletedDate { get; set; }
    public DateTime? CommittedDate { get; set; }

    // ==================== SECURITY ====================

    public bool IsChecksumValid { get; set; }
    public bool IsSignatureValid { get; set; }

    // ==================== CONTENT SUMMARY ====================

    public int SurveyCount { get; set; }
    public int BuildingCount { get; set; }
    public int PropertyUnitCount { get; set; }
    public int PersonCount { get; set; }
    public int ClaimCount { get; set; }
    public int DocumentCount { get; set; }
    public long TotalAttachmentSizeBytes { get; set; }

    /// <summary>
    /// Total record count across all entity types.
    /// </summary>
    public int TotalRecordCount =>
        SurveyCount + BuildingCount + PropertyUnitCount +
        PersonCount + ClaimCount + DocumentCount;

    // ==================== VOCABULARY ====================

    public bool IsVocabularyCompatible { get; set; }
    public string? VocabularyCompatibilityIssues { get; set; }

    // ==================== VALIDATION ====================

    public bool IsSchemaValid { get; set; }
    public int ValidationErrorCount { get; set; }
    public int ValidationWarningCount { get; set; }

    // ==================== CONFLICTS ====================

    public int PersonDuplicateCount { get; set; }
    public int PropertyDuplicateCount { get; set; }
    public int ConflictCount { get; set; }
    public bool AreConflictsResolved { get; set; }

    // ==================== IMPORT RESULTS ====================

    public int SuccessfulImportCount { get; set; }
    public int FailedImportCount { get; set; }
    public int SkippedRecordCount { get; set; }
    public decimal SuccessRate { get; set; }

    // ==================== ERROR INFO ====================

    public string? ErrorMessage { get; set; }

    // ==================== ARCHIVAL ====================

    public bool IsArchived { get; set; }

    // ==================== AUDIT ====================

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}
