namespace TRRCMS.Application.Import.Dtos;

/// <summary>
/// Focused report explaining why an import package is (or was) quarantined.
/// Returned by GET /packages/{id}/quarantine-report.
/// </summary>
public class QuarantineReportDto
{
    public Guid Id { get; set; }
    public string PackageNumber { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int CurrentStatus { get; set; }

    /// <summary>
    /// Primary human-readable reason stored when the package was quarantined.
    /// </summary>
    public string? QuarantineReason { get; set; }

    /// <summary>
    /// Derived category of the quarantine trigger.
    /// One of: ChecksumFailure, SignatureFailure, VocabularyVersionMismatch, SchemaInvalid, ManualQuarantine.
    /// </summary>
    public string QuarantineCategory { get; set; } = string.Empty;

    // ── integrity flags ──────────────────────────────────────────────────────
    public bool IsChecksumValid { get; set; }
    public bool IsSignatureValid { get; set; }
    public bool IsVocabularyCompatible { get; set; }
    public string? VocabularyCompatibilityIssues { get; set; }
    public bool IsSchemaValid { get; set; }
    public string? SchemaVersion { get; set; }

    /// <summary>
    /// Raw error log JSON (if any) from the validation pipeline.
    /// </summary>
    public string? ErrorLog { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }
}
