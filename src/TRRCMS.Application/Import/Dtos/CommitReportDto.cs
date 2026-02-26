namespace TRRCMS.Application.Import.Dtos;

/// <summary>
/// Data Transfer Object for the commit report generated after committing
/// staging data to production tables.
///
/// Contains per-entity-type counts, deduplication savings, conflict
/// resolutions applied, and any errors.
///
/// Record IDs per FSD Section 7:
///   - Buildings: 17-digit geographic code composed by Building.Create()
///   - PropertyUnits: BuildingId + UnitIdentifier composed by PropertyUnit.Create()
///   - Claims: CL-YYYY-NNNNNN via IClaimNumberGenerator
///   - Other entities: UUID primary key only (no additional Record ID)
///
/// Referenced in UC-003 Stage 4 — S17 (Commit) and S11 (Archive).
/// </summary>
public class CommitReportDto
{
    // ==================== PACKAGE IDENTITY ====================

    public Guid ImportPackageId { get; set; }
    public string PackageNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // ==================== COMMIT METADATA ====================

    public Guid CommittedByUserId { get; set; }
    public DateTime CommittedAtUtc { get; set; }
    public TimeSpan Duration { get; set; }

    // ==================== AGGREGATE COUNTS ====================

    public int TotalRecordsApproved { get; set; }
    public int TotalRecordsCommitted { get; set; }
    public int TotalRecordsFailed { get; set; }
    public int TotalRecordsSkipped { get; set; }

    /// <summary>Success rate as percentage (0-100).</summary>
    public decimal SuccessRate => TotalRecordsApproved > 0
        ? Math.Round((decimal)TotalRecordsCommitted / TotalRecordsApproved * 100, 2)
        : 0;

    // ==================== PER-ENTITY-TYPE BREAKDOWN ====================

    public CommitEntityTypeSummary Buildings { get; set; } = new();
    public CommitEntityTypeSummary PropertyUnits { get; set; } = new();
    public CommitEntityTypeSummary Persons { get; set; } = new();
    public CommitEntityTypeSummary Households { get; set; } = new();
    public CommitEntityTypeSummary PersonPropertyRelations { get; set; } = new();
    public CommitEntityTypeSummary Evidences { get; set; } = new();
    public CommitEntityTypeSummary Claims { get; set; } = new();
    public CommitEntityTypeSummary Surveys { get; set; } = new();

    // ==================== ATTACHMENT DEDUP (FR-D-9) ====================

    /// <summary>Number of attachment files that matched existing blobs by SHA-256 hash.</summary>
    public int DuplicateAttachmentsFound { get; set; }

    /// <summary>Bytes saved by reusing existing attachment blobs instead of re-storing.</summary>
    public long DeduplicationBytesSaved { get; set; }

    // ==================== CONFLICT RESOLUTIONS APPLIED ====================

    public int ConflictResolutionsApplied { get; set; }
    public int MergesPerformed { get; set; }

    // ==================== ARCHIVAL ====================

    public bool IsArchived { get; set; }
    public string? ArchivePath { get; set; }

    // ==================== ERRORS ====================

    /// <summary>List of errors encountered during commit (if any).</summary>
    public List<CommitErrorDto> Errors { get; set; } = new();

    /// <summary>True if commit completed without any errors.</summary>
    public bool IsFullySuccessful => Errors.Count == 0 && TotalRecordsFailed == 0;
}

/// <summary>
/// Commit summary for a single entity type.
/// </summary>
public class CommitEntityTypeSummary
{
    public string EntityType { get; set; } = string.Empty;
    public int Approved { get; set; }
    public int Committed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }

    /// <summary>
    /// Mapping of staging entity IDs to production entity IDs (for traceability).
    /// Key = StagingEntity.OriginalEntityId, Value = Production Entity.Id.
    /// </summary>
    public Dictionary<Guid, Guid> IdMappings { get; set; } = new();
}

/// <summary>
/// Individual error encountered during the commit process.
/// </summary>
public class CommitErrorDto
{
    public string EntityType { get; set; } = string.Empty;
    public Guid? StagingEntityId { get; set; }
    public Guid? OriginalEntityId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
