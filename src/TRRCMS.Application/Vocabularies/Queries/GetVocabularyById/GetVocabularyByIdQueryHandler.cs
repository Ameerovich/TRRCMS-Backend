using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Mappings;

namespace TRRCMS.Application.Vocabularies.Queries.GetVocabularyById;

public class GetVocabularyByIdQueryHandler : IRequestHandler<GetVocabularyByIdQuery, VocabularyDto?>
{
    private readonly IVocabularyRepository _vocabularyRepository;

    public GetVocabularyByIdQueryHandler(IVocabularyRepository vocabularyRepository)
    {
        _vocabularyRepository = vocabularyRepository;
    }

    public async Task<VocabularyDto?> Handle(GetVocabularyByIdQuery request, CancellationToken cancellationToken)
    {
        var vocabulary = await _vocabularyRepository.GetByIdAsync(request.Id, cancellationToken);

        return vocabulary is null ? null : VocabularyMappingHelper.MapToDto(vocabulary);
    }
}
