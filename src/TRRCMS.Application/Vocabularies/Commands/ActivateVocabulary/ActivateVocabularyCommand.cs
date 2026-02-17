using MediatR;
using TRRCMS.Application.Vocabularies.Dtos;

namespace TRRCMS.Application.Vocabularies.Commands.ActivateVocabulary;

public record ActivateVocabularyCommand : IRequest<VocabularyDto>
{
    public Guid Id { get; init; }
}
