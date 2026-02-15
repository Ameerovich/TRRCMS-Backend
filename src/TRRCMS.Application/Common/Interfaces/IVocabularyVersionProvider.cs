namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Provides vocabulary version information for import compatibility checking.
/// Replaces hardcoded ServerVocabularyVersions from appsettings.json.
/// </summary>
public interface IVocabularyVersionProvider
{
    /// <summary>
    /// Get the current version string for a specific vocabulary.
    /// </summary>
    Task<string?> GetCurrentVersionAsync(string vocabularyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all current vocabulary versions as a dictionary (name → version string).
    /// Used by import pipeline for compatibility checking.
    /// </summary>
    Task<Dictionary<string, string>> GetAllCurrentVersionsAsync(CancellationToken cancellationToken = default);
}
