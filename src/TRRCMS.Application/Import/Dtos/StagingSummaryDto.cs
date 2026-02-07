namespace TRRCMS.Application.Import.Dtos;

/// <summary>
/// Data Transfer Object for the staging and validation summary.
/// Returned by GetStagingSummaryQuery and StagePackageCommand.
/// Provides per-entity-type counts and per-level validation results.
/// </summary>
public class StagingSummaryDto
{
    // ==================== PACKAGE IDENTITY ====================

    public Guid ImportPackageId { get; set; }
    public string PackageNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // ==================== STAGING COUNTS (per entity type) ====================

    public EntityTypeSummary Surveys { get; set; } = new();
    public EntityTypeSummary Buildings { get; set; } = new();
    public EntityTypeSummary PropertyUnits { get; set; } = new();
    public EntityTypeSummary Persons { get; set; } = new();
    public EntityTypeSummary Households { get; set; } = new();
    public EntityTypeSummary PersonPropertyRelations { get; set; } = new();
    public EntityTypeSummary Evidences { get; set; } = new();
    public EntityTypeSummary Claims { get; set; } = new();

    // ==================== AGGREGATE COUNTS ====================

    public int TotalRecords { get; set; }
    public int TotalValid { get; set; }
    public int TotalInvalid { get; set; }
    public int TotalWarning { get; set; }
    public int TotalSkipped { get; set; }
    public int TotalPending { get; set; }

    /// <summary>True if no blocking errors were found across all entities.</summary>
    public bool IsClean => TotalInvalid == 0;

    // ==================== VALIDATION LEVEL RESULTS ====================

    /// <summary>Per-level validator results (8 levels).</summary>
    public List<ValidationLevelResultDto> LevelResults { get; set; } = new();

    // ==================== ATTACHMENT SUMMARY ====================

    public int AttachmentFilesExtracted { get; set; }
    public long AttachmentBytesExtracted { get; set; }
}

/// <summary>
/// Summary of staging records for a single entity type.
/// </summary>
public class EntityTypeSummary
{
    public string EntityType { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Valid { get; set; }
    public int Invalid { get; set; }
    public int Warning { get; set; }
    public int Skipped { get; set; }
    public int Pending { get; set; }
}

/// <summary>
/// Result of a single validation level.
/// </summary>
public class ValidationLevelResultDto
{
    public int Level { get; set; }
    public string ValidatorName { get; set; } = string.Empty;
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int RecordsChecked { get; set; }
    public double DurationMs { get; set; }
}
