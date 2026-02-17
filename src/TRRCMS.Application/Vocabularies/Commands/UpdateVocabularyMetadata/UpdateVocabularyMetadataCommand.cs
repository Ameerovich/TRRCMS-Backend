using MediatR;
using TRRCMS.Application.Vocabularies.Dtos;

namespace TRRCMS.Application.Vocabularies.Commands.UpdateVocabularyMetadata;

public record UpdateVocabularyMetadataCommand : IRequest<VocabularyDto>
{
    public Guid Id { get; init; }
    public string DisplayNameArabic { get; init; } = string.Empty;
    public string? DisplayNameEnglish { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
}
