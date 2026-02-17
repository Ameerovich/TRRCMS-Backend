using MediatR;
using TRRCMS.Application.Vocabularies.Dtos;

namespace TRRCMS.Application.Vocabularies.Queries.GetVocabularyById;

public record GetVocabularyByIdQuery : IRequest<VocabularyDto?>
{
    public Guid Id { get; init; }
}
