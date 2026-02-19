namespace TRRCMS.Application.Sync.DTOs;

/// <summary>
/// Lightweight representation of a controlled vocabulary (code list) sent to the
/// tablet during a sync download.
///
/// The tablet stores these lists locally and uses them to populate drop-down
/// fields in the field survey form.  Semantic versioning (MAJOR.MINOR.PATCH)
/// allows the tablet to detect whether its cached copy is still current:
/// <list type="bullet">
///   <item>MAJOR mismatch → quarantine; sync blocked until app is updated.</item>
///   <item>MINOR/PATCH mismatch → advisory warning; sync can proceed.</item>
/// </list>
///
/// FSD: FR-V-1 through FR-V-4 (Vocabulary Versioning).
/// </summary>
public sealed record SyncVocabularyDto
{
    /// <summary>
    /// Canonical vocabulary name / identifier (e.g., <c>"ownership_type"</c>).
    /// Matches <c>Vocabulary.VocabularyName</c> on the server.
    /// </summary>
    public string VocabularyName { get; init; } = string.Empty;

    /// <summary>
    /// Arabic display label used in the tablet UI drop-down headers.
    /// </summary>
    public string DisplayNameArabic { get; init; } = string.Empty;

    /// <summary>
    /// Optional English display label for the vocabulary category.
    /// </summary>
    public string? DisplayNameEnglish { get; init; }

    /// <summary>
    /// Semantic version of this vocabulary snapshot (e.g., <c>"2.1.0"</c>).
    /// The tablet compares this value against its cached version to decide
    /// whether to refresh local data.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// JSON array of vocabulary items serialised directly from the server store.
    /// Format: <c>[{"code":"1","labelAr":"مالك","labelEn":"Owner","displayOrder":1}, ...]</c>.
    /// The tablet deserialises this string and stores items in its local SQLite database.
    /// </summary>
    public string ValuesJson { get; init; } = "[]";
}
