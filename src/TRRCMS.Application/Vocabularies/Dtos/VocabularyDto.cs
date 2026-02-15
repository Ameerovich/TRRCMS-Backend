namespace TRRCMS.Application.Vocabularies.Dtos;

/// <summary>
/// DTO for Vocabulary entity — returned to clients for dropdown population.
/// Clients cache this data and use integer codes for all subsequent API calls.
/// </summary>
public class VocabularyDto
{
    public Guid Id { get; set; }
    public string VocabularyName { get; set; } = string.Empty;
    public string DisplayNameArabic { get; set; } = string.Empty;
    public string? DisplayNameEnglish { get; set; }
    public string? Description { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public int ValueCount { get; set; }

    /// <summary>
    /// Parsed vocabulary values with bilingual labels.
    /// </summary>
    public List<VocabularyValueDto> Values { get; set; } = new();
}
