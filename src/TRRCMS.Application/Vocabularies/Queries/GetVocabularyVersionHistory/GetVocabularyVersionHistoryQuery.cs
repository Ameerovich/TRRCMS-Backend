using MediatR;
using TRRCMS.Application.Vocabularies.Dtos;

namespace TRRCMS.Application.Vocabularies.Queries.GetVocabularyVersionHistory;

public record GetVocabularyVersionHistoryQuery : IRequest<List<VocabularyDto>>
{
    public string VocabularyName { get; init; } = string.Empty;
}
