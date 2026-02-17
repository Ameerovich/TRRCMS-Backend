namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Validates integer codes against the vocabulary table (cached in-memory).
/// After initial seeding from C# enums, the vocabulary table becomes the source of truth
/// for all dropdown validation — enabling admin-managed values via the UI.
/// </summary>
public interface IVocabularyValidationService
{
    /// <summary>
    /// Check if a code is valid for the given vocabulary.
    /// Synchronous — the cache is in-memory, no DB call needed.
    /// </summary>
    bool IsValidCode(string vocabularyName, int code);

    /// <summary>
    /// Get all valid codes for a vocabulary.
    /// Returns empty set if vocabulary not found.
    /// </summary>
    IReadOnlySet<int> GetValidCodes(string vocabularyName);

    /// <summary>
    /// Invalidate the in-memory cache. Called after vocabulary create/update/activate/deactivate.
    /// Next call to IsValidCode or GetValidCodes will reload from DB.
    /// </summary>
    void InvalidateCache();

    /// <summary>
    /// Explicitly load the cache from the database. Called at startup after seeding.
    /// </summary>
    Task WarmupAsync(CancellationToken cancellationToken = default);
}
