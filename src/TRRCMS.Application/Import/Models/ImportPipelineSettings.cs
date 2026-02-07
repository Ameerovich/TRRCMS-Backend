namespace TRRCMS.Application.Import.Models;

/// <summary>
/// Configuration settings for the import pipeline.
/// Bound from appsettings.json section "ImportPipeline".
/// 
/// Add to appsettings.json:
/// "ImportPipeline": {
///   "MaxUploadSizeMB": 500,
///   "ArchiveBasePath": "archives",
///   "PackageStoragePath": "wwwroot/packages",
///   "AllowedExtensions": [".uhc"],
///   "RequireDigitalSignature": false,
///   "StagingRetentionDays": 90,
///   "DuplicateDetection": {
///     "PersonHighConfidenceThreshold": 90,
///     "PersonMediumConfidenceThreshold": 70,
///     "PropertyProximityMeters": 50
///   },
///   "ServerVocabularyVersions": {
///     "ownership_type": "1.0.0",
///     "document_type": "1.0.0",
///     "building_type": "1.0.0",
///     "claim_type": "1.0.0"
///   }
/// }
/// </summary>
public class ImportPipelineSettings
{
    public const string SectionName = "ImportPipeline";

    /// <summary>Maximum upload file size in megabytes (default 500 MB).</summary>
    public int MaxUploadSizeMB { get; set; } = 500;

    /// <summary>Maximum upload file size in bytes (computed).</summary>
    public long MaxUploadSizeBytes => (long)MaxUploadSizeMB * 1024 * 1024;

    /// <summary>Base path for archived .uhc packages (archives/YYYY/MM/[id].uhc).</summary>
    public string ArchiveBasePath { get; set; } = "archives";

    /// <summary>Temporary storage path for uploaded .uhc packages before processing.</summary>
    public string PackageStoragePath { get; set; } = "wwwroot/packages";

    /// <summary>Allowed file extensions for upload.</summary>
    public string[] AllowedExtensions { get; set; } = { ".uhc" };

    /// <summary>Whether digital signature is required. False = skip signature check.</summary>
    public bool RequireDigitalSignature { get; set; } = false;

    /// <summary>Days to retain staging data after commit (0 = delete immediately).</summary>
    public int StagingRetentionDays { get; set; } = 90;

    /// <summary>Duplicate detection threshold settings.</summary>
    public DuplicateDetectionSettings DuplicateDetection { get; set; } = new();

    /// <summary>
    /// Server's current vocabulary versions.
    /// Key = domain name, Value = semver string.
    /// Used for compatibility checking against package vocabulary versions.
    /// </summary>
    public Dictionary<string, string> ServerVocabularyVersions { get; set; } = new();
}

/// <summary>
/// Duplicate detection threshold settings.
/// </summary>
public class DuplicateDetectionSettings
{
    /// <summary>Person similarity score ≥ this → High confidence (auto-flag).</summary>
    public int PersonHighConfidenceThreshold { get; set; } = 90;

    /// <summary>Person similarity score ≥ this → Medium confidence (manual review).</summary>
    public int PersonMediumConfidenceThreshold { get; set; } = 70;

    /// <summary>Property spatial proximity threshold in meters.</summary>
    public int PropertyProximityMeters { get; set; } = 50;
}
