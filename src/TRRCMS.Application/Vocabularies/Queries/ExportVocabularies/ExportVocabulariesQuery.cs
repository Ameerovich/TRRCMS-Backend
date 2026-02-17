using MediatR;
using TRRCMS.Application.Vocabularies.Dtos;

namespace TRRCMS.Application.Vocabularies.Queries.ExportVocabularies;

/// <summary>
/// Export all current vocabularies as a snapshot for disaster recovery.
/// Returns full vocabulary data including system flags needed for reimport.
/// </summary>
public record ExportVocabulariesQuery : IRequest<List<VocabularyExportDto>>;

/// <summary>
/// Extended vocabulary DTO for export/import — includes fields needed for reimport
/// that are not exposed in the regular public API (IsSystemVocabulary, AllowCustomValues).
/// </summary>
public class VocabularyExportDto
{
    public string VocabularyName { get; set; } = string.Empty;
    public string DisplayNameArabic { get; set; } = string.Empty;
    public string? DisplayNameEnglish { get; set; }
    public string? Description { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemVocabulary { get; set; }
    public bool AllowCustomValues { get; set; }
    public List<VocabularyValueDto> Values { get; set; } = new();
}
