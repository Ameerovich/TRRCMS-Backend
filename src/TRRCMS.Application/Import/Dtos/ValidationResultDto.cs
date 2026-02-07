namespace TRRCMS.Application.Import.Dtos;

/// <summary>
/// Data Transfer Object for package validation results.
/// Returned after integrity verification during upload.
/// </summary>
public class PackageValidationResultDto
{
    /// <summary>Overall validation passed (checksum + signature + vocabulary).</summary>
    public bool IsValid { get; set; }

    /// <summary>SHA-256 checksum verification result.</summary>
    public bool IsChecksumValid { get; set; }

    /// <summary>Digital signature verification result.</summary>
    public bool IsSignatureValid { get; set; }

    /// <summary>Schema version is recognized and supported.</summary>
    public bool IsSchemaValid { get; set; }

    /// <summary>Vocabulary versions are compatible with the server.</summary>
    public bool IsVocabularyCompatible { get; set; }

    /// <summary>Vocabulary compatibility warnings (MINOR mismatches).</summary>
    public List<string> VocabularyWarnings { get; set; } = new();

    /// <summary>Blocking validation errors.</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Non-blocking warnings.</summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Combined response from the UploadPackage command.
/// Includes the created ImportPackage DTO and the validation results.
/// </summary>
public class UploadPackageResultDto
{
    /// <summary>The created import package record.</summary>
    public ImportPackageDto Package { get; set; } = null!;

    /// <summary>Validation results from integrity checks.</summary>
    public PackageValidationResultDto ValidationResult { get; set; } = null!;

    /// <summary>
    /// Whether the package was quarantined due to integrity failures.
    /// If true, the package cannot proceed to staging.
    /// </summary>
    public bool IsQuarantined { get; set; }

    /// <summary>
    /// Whether this package was already imported (idempotency hit).
    /// If true, Package contains the existing record and no new import was created.
    /// </summary>
    public bool IsDuplicatePackage { get; set; }

    /// <summary>
    /// Human-readable message summarizing the upload result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
