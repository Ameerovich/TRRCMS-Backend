namespace TRRCMS.Application.Import.Dtos;

/// <summary>
/// Data Transfer Object returned by the DetectDuplicatesCommand.
/// Summarizes duplicate detection results for the API response.
/// </summary>
public class DuplicateDetectionResultDto
{
    // ==================== PACKAGE IDENTITY ====================

    public Guid ImportPackageId { get; set; }
    public string PackageNumber { get; set; } = string.Empty;

    /// <summary>Package status after detection (ReviewingConflicts or ReadyToCommit).</summary>
    public string Status { get; set; } = string.Empty;

    // ==================== DETECTION SUMMARY ====================

    public int PersonDuplicatesFound { get; set; }
    public int PropertyDuplicatesFound { get; set; }
    public int TotalConflictsCreated { get; set; }

    /// <summary>True if no duplicates were found — package proceeds directly to ReadyToCommit.</summary>
    public bool IsClean => TotalConflictsCreated == 0;

    // ==================== SCAN METRICS ====================

    public int PersonsScanned { get; set; }
    public int BuildingsScanned { get; set; }
    public double DurationMs { get; set; }

    // ==================== CONFLICT IDS (for immediate navigation) ====================

    /// <summary>IDs of created ConflictResolution records for direct API access.</summary>
    public List<Guid> ConflictIds { get; set; } = new();

    /// <summary>Human-readable summary message.</summary>
    public string Message { get; set; } = string.Empty;
}
