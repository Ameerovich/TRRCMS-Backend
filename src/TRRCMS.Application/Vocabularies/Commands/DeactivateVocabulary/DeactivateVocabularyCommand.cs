using MediatR;
using TRRCMS.Application.Vocabularies.Dtos;

namespace TRRCMS.Application.Vocabularies.Commands.DeactivateVocabulary;

public record DeactivateVocabularyCommand : IRequest<VocabularyDto>
{
    public Guid Id { get; init; }
}
