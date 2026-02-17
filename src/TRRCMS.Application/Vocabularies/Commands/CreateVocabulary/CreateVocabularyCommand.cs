using MediatR;
using TRRCMS.Application.Vocabularies.Dtos;

namespace TRRCMS.Application.Vocabularies.Commands.CreateVocabulary;

public record CreateVocabularyCommand : IRequest<VocabularyDto>
{
    public string VocabularyName { get; init; } = string.Empty;
    public string DisplayNameArabic { get; init; } = string.Empty;
    public string? DisplayNameEnglish { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public bool IsSystemVocabulary { get; init; }
    public bool AllowCustomValues { get; init; }
    public List<VocabularyValueDto> Values { get; init; } = new();
}
