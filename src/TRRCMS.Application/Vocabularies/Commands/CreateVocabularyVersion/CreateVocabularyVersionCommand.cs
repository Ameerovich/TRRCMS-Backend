using MediatR;
using TRRCMS.Application.Vocabularies.Dtos;

namespace TRRCMS.Application.Vocabularies.Commands.CreateVocabularyVersion;

public record CreateVocabularyVersionCommand : IRequest<VocabularyDto>
{
    public Guid VocabularyId { get; init; }
    public string VersionType { get; init; } = string.Empty;
    public List<VocabularyValueDto> Values { get; init; } = new();
    public string ChangeLog { get; init; } = string.Empty;
}
