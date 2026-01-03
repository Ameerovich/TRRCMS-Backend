using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Vocabulary entity - manages controlled vocabularies/code lists
/// Supports semantic versioning (MAJOR.MINOR.PATCH)
/// </summary>
public class Vocabulary : BaseAuditableEntity
{
    // ==================== VOCABULARY IDENTIFICATION ====================

    /// <summary>
    /// Vocabulary name/identifier (e.g., "ownership_type", "document_type")
    /// Unique identifier for the vocabulary category
    /// </summary>
    public string VocabularyName { get; private set; }

    /// <summary>
    /// Display name in Arabic (الاسم المعروض)
    /// </summary>
    public string DisplayNameArabic { get; private set; }

    /// <summary>
    /// Display name in English (optional)
    /// </summary>
    public string? DisplayNameEnglish { get; private set; }

    /// <summary>
    /// Description of this vocabulary category
    /// </summary>
    public string? Description { get; private set; }

    // ==================== VERSIONING ====================

    /// <summary>
    /// Semantic version: MAJOR.MINOR.PATCH
    /// MAJOR: Breaking changes (incompatible)
    /// MINOR: New values added (backward compatible)
    /// PATCH: Label changes only (fully compatible)
    /// </summary>
    public string Version { get; private set; }

    /// <summary>
    /// Major version number
    /// </summary>
    public int MajorVersion { get; private set; }

    /// <summary>
    /// Minor version number
    /// </summary>
    public int MinorVersion { get; private set; }

    /// <summary>
    /// Patch version number
    /// </summary>
    public int PatchVersion { get; private set; }

    /// <summary>
    /// Date when this version was created
    /// </summary>
    public DateTime VersionDate { get; private set; }

    /// <summary>
    /// Indicates if this is the current/active version
    /// </summary>
    public bool IsCurrentVersion { get; private set; }

    /// <summary>
    /// Reference to previous version (if this is an update)
    /// </summary>
    public Guid? PreviousVersionId { get; private set; }

    // ==================== VOCABULARY VALUES ====================

    /// <summary>
    /// Vocabulary values stored as JSON
    /// Format: [{"code": "1", "labelAr": "مالك", "labelEn": "Owner", "description": "...", "displayOrder": 1}, ...]
    /// </summary>
    public string ValuesJson { get; private set; }

    /// <summary>
    /// Number of values in this vocabulary
    /// </summary>
    public int ValueCount { get; private set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Category or grouping (e.g., "Demographics", "Property", "Legal")
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    /// Indicates if this vocabulary is system-defined (cannot be deleted)
    /// </summary>
    public bool IsSystemVocabulary { get; private set; }

    /// <summary>
    /// Indicates if users can add custom values (in addition to predefined ones)
    /// </summary>
    public bool AllowCustomValues { get; private set; }

    /// <summary>
    /// Indicates if this vocabulary is mandatory (must be used in forms)
    /// </summary>
    public bool IsMandatory { get; private set; }

    /// <summary>
    /// Indicates if this vocabulary is active/published
    /// </summary>
    public bool IsActive { get; private set; }

    // ==================== IMPORT COMPATIBILITY ====================

    /// <summary>
    /// Minimum compatible version for imports
    /// Imports with earlier versions will be rejected
    /// </summary>
    public string? MinimumCompatibleVersion { get; private set; }

    /// <summary>
    /// Changelog describing changes in this version
    /// </summary>
    public string? ChangeLog { get; private set; }

    // ==================== USAGE TRACKING ====================

    /// <summary>
    /// Date when vocabulary was last used
    /// </summary>
    public DateTime? LastUsedDate { get; private set; }

    /// <summary>
    /// Count of how many times this vocabulary has been used
    /// </summary>
    public int UsageCount { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Previous version of this vocabulary
    /// </summary>
    public virtual Vocabulary? PreviousVersion { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Vocabulary() : base()
    {
        VocabularyName = string.Empty;
        DisplayNameArabic = string.Empty;
        Version = "1.0.0";
        MajorVersion = 1;
        MinorVersion = 0;
        PatchVersion = 0;
        ValuesJson = "[]";
        ValueCount = 0;
        IsSystemVocabulary = false;
        AllowCustomValues = false;
        IsMandatory = false;
        IsActive = true;
        IsCurrentVersion = true;
        UsageCount = 0;
    }

    /// <summary>
    /// Create new vocabulary
    /// </summary>
    public static Vocabulary Create(
        string vocabularyName,
        string displayNameArabic,
        string? displayNameEnglish,
        string? description,
        string valuesJson,
        bool isSystemVocabulary,
        bool allowCustomValues,
        string? category,
        Guid createdByUserId)
    {
        var vocabulary = new Vocabulary
        {
            VocabularyName = vocabularyName,
            DisplayNameArabic = displayNameArabic,
            DisplayNameEnglish = displayNameEnglish,
            Description = description,
            ValuesJson = valuesJson,
            ValueCount = CountValuesInJson(valuesJson),
            Version = "1.0.0",
            MajorVersion = 1,
            MinorVersion = 0,
            PatchVersion = 0,
            VersionDate = DateTime.UtcNow,
            IsSystemVocabulary = isSystemVocabulary,
            AllowCustomValues = allowCustomValues,
            Category = category,
            IsActive = true,
            IsCurrentVersion = true,
            UsageCount = 0
        };

        vocabulary.MarkAsCreated(createdByUserId);

        return vocabulary;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Create new version with added values (MINOR version bump)
    /// </summary>
    public Vocabulary CreateMinorVersion(string newValuesJson, string changeLog, Guid createdByUserId)
    {
        // Mark current version as not current
        IsCurrentVersion = false;

        var newVersion = new Vocabulary
        {
            VocabularyName = VocabularyName,
            DisplayNameArabic = DisplayNameArabic,
            DisplayNameEnglish = DisplayNameEnglish,
            Description = Description,
            ValuesJson = newValuesJson,
            ValueCount = CountValuesInJson(newValuesJson),
            MajorVersion = MajorVersion,
            MinorVersion = MinorVersion + 1,
            PatchVersion = 0,
            Version = $"{MajorVersion}.{MinorVersion + 1}.0",
            VersionDate = DateTime.UtcNow,
            PreviousVersionId = Id,
            IsSystemVocabulary = IsSystemVocabulary,
            AllowCustomValues = AllowCustomValues,
            IsMandatory = IsMandatory,
            Category = Category,
            IsActive = true,
            IsCurrentVersion = true,
            MinimumCompatibleVersion = MinimumCompatibleVersion,
            ChangeLog = changeLog,
            UsageCount = 0
        };

        newVersion.MarkAsCreated(createdByUserId);

        return newVersion;
    }

    /// <summary>
    /// Create new version with label changes only (PATCH version bump)
    /// </summary>
    public Vocabulary CreatePatchVersion(string newValuesJson, string changeLog, Guid createdByUserId)
    {
        IsCurrentVersion = false;

        var newVersion = new Vocabulary
        {
            VocabularyName = VocabularyName,
            DisplayNameArabic = DisplayNameArabic,
            DisplayNameEnglish = DisplayNameEnglish,
            Description = Description,
            ValuesJson = newValuesJson,
            ValueCount = CountValuesInJson(newValuesJson),
            MajorVersion = MajorVersion,
            MinorVersion = MinorVersion,
            PatchVersion = PatchVersion + 1,
            Version = $"{MajorVersion}.{MinorVersion}.{PatchVersion + 1}",
            VersionDate = DateTime.UtcNow,
            PreviousVersionId = Id,
            IsSystemVocabulary = IsSystemVocabulary,
            AllowCustomValues = AllowCustomValues,
            IsMandatory = IsMandatory,
            Category = Category,
            IsActive = true,
            IsCurrentVersion = true,
            MinimumCompatibleVersion = MinimumCompatibleVersion,
            ChangeLog = changeLog,
            UsageCount = 0
        };

        newVersion.MarkAsCreated(createdByUserId);

        return newVersion;
    }

    /// <summary>
    /// Create new version with breaking changes (MAJOR version bump)
    /// </summary>
    public Vocabulary CreateMajorVersion(string newValuesJson, string changeLog, Guid createdByUserId)
    {
        IsCurrentVersion = false;

        var newVersion = new Vocabulary
        {
            VocabularyName = VocabularyName,
            DisplayNameArabic = DisplayNameArabic,
            DisplayNameEnglish = DisplayNameEnglish,
            Description = Description,
            ValuesJson = newValuesJson,
            ValueCount = CountValuesInJson(newValuesJson),
            MajorVersion = MajorVersion + 1,
            MinorVersion = 0,
            PatchVersion = 0,
            Version = $"{MajorVersion + 1}.0.0",
            VersionDate = DateTime.UtcNow,
            PreviousVersionId = Id,
            IsSystemVocabulary = IsSystemVocabulary,
            AllowCustomValues = AllowCustomValues,
            IsMandatory = IsMandatory,
            Category = Category,
            IsActive = true,
            IsCurrentVersion = true,
            MinimumCompatibleVersion = $"{MajorVersion + 1}.0.0", // Breaking changes require this version
            ChangeLog = changeLog,
            UsageCount = 0
        };

        newVersion.MarkAsCreated(createdByUserId);

        return newVersion;
    }

    /// <summary>
    /// Update vocabulary metadata
    /// </summary>
    public void UpdateMetadata(
        string displayNameArabic,
        string? displayNameEnglish,
        string? description,
        string? category,
        Guid modifiedByUserId)
    {
        DisplayNameArabic = displayNameArabic;
        DisplayNameEnglish = displayNameEnglish;
        Description = description;
        Category = category;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Activate vocabulary
    /// </summary>
    public void Activate(Guid modifiedByUserId)
    {
        IsActive = true;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Deactivate vocabulary
    /// </summary>
    public void Deactivate(Guid modifiedByUserId)
    {
        IsActive = false;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Record usage of this vocabulary
    /// </summary>
    public void RecordUsage()
    {
        UsageCount++;
        LastUsedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Set minimum compatible version
    /// </summary>
    public void SetMinimumCompatibleVersion(string minVersion, Guid modifiedByUserId)
    {
        MinimumCompatibleVersion = minVersion;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Check if a given version is compatible with this vocabulary
    /// </summary>
    public bool IsVersionCompatible(string versionToCheck)
    {
        if (string.IsNullOrWhiteSpace(MinimumCompatibleVersion))
            return true; // No restrictions

        // Parse versions and compare
        var minParts = MinimumCompatibleVersion.Split('.');
        var checkParts = versionToCheck.Split('.');

        if (minParts.Length != 3 || checkParts.Length != 3)
            return false;

        if (!int.TryParse(minParts[0], out int minMajor) ||
            !int.TryParse(minParts[1], out int minMinor) ||
            !int.TryParse(minParts[2], out int minPatch) ||
            !int.TryParse(checkParts[0], out int checkMajor) ||
            !int.TryParse(checkParts[1], out int checkMinor) ||
            !int.TryParse(checkParts[2], out int checkPatch))
        {
            return false;
        }

        // Check compatibility
        if (checkMajor < minMajor) return false;
        if (checkMajor > minMajor) return true;

        // Same major version
        if (checkMinor < minMinor) return false;
        if (checkMinor > minMinor) return true;

        // Same major and minor version
        return checkPatch >= minPatch;
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Count number of values in JSON array
    /// Simple implementation - can be enhanced
    /// </summary>
    private static int CountValuesInJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
            return 0;

        // Simple count of objects in array (count opening braces)
        int count = 0;
        bool inString = false;

        foreach (char c in json)
        {
            if (c == '"') inString = !inString;
            if (!inString && c == '{') count++;
        }

        return count;
    }
}