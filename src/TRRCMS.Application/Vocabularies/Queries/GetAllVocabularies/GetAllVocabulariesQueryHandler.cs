using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Mappings;

namespace TRRCMS.Application.Vocabularies.Queries.GetAllVocabularies;

public class GetAllVocabulariesQueryHandler : IRequestHandler<GetAllVocabulariesQuery, List<VocabularyDto>>
{
    private readonly IVocabularyRepository _vocabularyRepository;

    public GetAllVocabulariesQueryHandler(IVocabularyRepository vocabularyRepository)
    {
        _vocabularyRepository = vocabularyRepository;
    }

    public async Task<List<VocabularyDto>> Handle(GetAllVocabulariesQuery request, CancellationToken cancellationToken)
    {
        var vocabularies = !string.IsNullOrWhiteSpace(request.Category)
            ? await _vocabularyRepository.GetByCategoryAsync(request.Category, cancellationToken)
            : await _vocabularyRepository.GetAllCurrentAsync(cancellationToken);

        return VocabularyMappingHelper.MapToDtoList(vocabularies);
    }
}
