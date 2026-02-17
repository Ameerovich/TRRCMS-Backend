using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Mappings;

namespace TRRCMS.Application.Vocabularies.Queries.GetVocabularyVersionHistory;

public class GetVocabularyVersionHistoryQueryHandler : IRequestHandler<GetVocabularyVersionHistoryQuery, List<VocabularyDto>>
{
    private readonly IVocabularyRepository _vocabularyRepository;

    public GetVocabularyVersionHistoryQueryHandler(IVocabularyRepository vocabularyRepository)
    {
        _vocabularyRepository = vocabularyRepository;
    }

    public async Task<List<VocabularyDto>> Handle(GetVocabularyVersionHistoryQuery request, CancellationToken cancellationToken)
    {
        var vocabularies = await _vocabularyRepository.GetVersionHistoryAsync(request.VocabularyName, cancellationToken);

        return VocabularyMappingHelper.MapToDtoList(vocabularies);
    }
}
