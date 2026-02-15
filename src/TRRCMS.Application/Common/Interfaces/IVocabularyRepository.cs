using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Vocabulary repository interface for managing controlled vocabulary data.
/// Supports versioned vocabulary lookups and category filtering.
/// </summary>
public interface IVocabularyRepository
{
    /// <summary>
    /// Get vocabulary by name. If currentOnly is true, returns only the current version.
    /// </summary>
    Task<Vocabulary?> GetByNameAsync(string vocabularyName, bool currentOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all current (active) vocabularies.
    /// </summary>
    Task<List<Vocabulary>> GetAllCurrentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all current vocabularies filtered by category.
    /// </summary>
    Task<List<Vocabulary>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get version history for a vocabulary (all versions ordered by version date descending).
    /// </summary>
    Task<List<Vocabulary>> GetVersionHistoryAsync(string vocabularyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a vocabulary with the given name exists.
    /// </summary>
    Task<bool> ExistsAsync(string vocabularyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get vocabulary by Id.
    /// </summary>
    Task<Vocabulary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new vocabulary.
    /// </summary>
    Task<Vocabulary> AddAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing vocabulary.
    /// </summary>
    Task UpdateAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default);
}
