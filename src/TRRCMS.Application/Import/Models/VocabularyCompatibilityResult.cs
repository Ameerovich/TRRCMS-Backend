namespace TRRCMS.Application.Import.Models;

/// <summary>
/// Result of vocabulary compatibility check between a .uhc package
/// and the server's current vocabulary versions.
/// 
/// Compatibility rules (semver):
/// - MAJOR mismatch → Incompatible (quarantine the package)
/// - MINOR addition → Compatible with warnings (new codes the server doesn't know)
/// - PATCH change   → Fully compatible (label changes only)
/// 
/// Referenced in FR-D-3 (Validation & Verification).
/// </summary>
public class VocabularyCompatibilityResult
{
    /// <summary>
    /// Overall compatibility: true if all vocabularies are compatible (no MAJOR mismatches).
    /// </summary>
    public bool IsCompatible { get; set; }

    /// <summary>
    /// True if all versions match exactly (no warnings).
    /// </summary>
    public bool IsFullyCompatible { get; set; }

    /// <summary>
    /// Individual check results per vocabulary domain.
    /// </summary>
    public List<VocabularyCheckItem> Items { get; set; } = new();

    /// <summary>
    /// Human-readable summary of compatibility issues (for logging / UI).
    /// Null if fully compatible.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// JSON representation of all issues for storage in ImportPackage.VocabularyCompatibilityIssues.
    /// </summary>
    public string? IssuesJson { get; set; }

    /// <summary>
    /// JSON representation of package vocabulary versions for storage in ImportPackage.VocabularyVersions.
    /// </summary>
    public string? VersionsJson { get; set; }
}

/// <summary>
/// Individual vocabulary domain compatibility check result.
/// </summary>
public class VocabularyCheckItem
{
    /// <summary>
    /// Vocabulary domain name (e.g. "ownership_type", "document_type").
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Version in the .uhc package.
    /// </summary>
    public string PackageVersion { get; set; } = string.Empty;

    /// <summary>
    /// Version on the server.
    /// </summary>
    public string ServerVersion { get; set; } = string.Empty;

    /// <summary>
    /// Compatibility level for this domain.
    /// </summary>
    public VocabularyCompatibilityLevel Level { get; set; }

    /// <summary>
    /// Human-readable message.
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Vocabulary compatibility level based on semver comparison.
/// </summary>
public enum VocabularyCompatibilityLevel
{
    /// <summary>Exact version match.</summary>
    Identical = 0,

    /// <summary>PATCH difference only — fully compatible.</summary>
    PatchDifference = 1,

    /// <summary>MINOR difference — compatible with warnings (new codes).</summary>
    MinorDifference = 2,

    /// <summary>MAJOR difference — incompatible, package must be quarantined.</summary>
    MajorDifference = 3,

    /// <summary>Server does not have this vocabulary domain.</summary>
    UnknownDomain = 4
}
