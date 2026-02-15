using System.Text.Json;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Domain.Entities;

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
        List<Vocabulary> vocabularies;

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            vocabularies = await _vocabularyRepository.GetByCategoryAsync(request.Category, cancellationToken);
        }
        else
        {
            vocabularies = await _vocabularyRepository.GetAllCurrentAsync(cancellationToken);
        }

        return vocabularies.Select(MapToDto).ToList();
    }

    private static VocabularyDto MapToDto(Vocabulary vocabulary)
    {
        var dto = new VocabularyDto
        {
            Id = vocabulary.Id,
            VocabularyName = vocabulary.VocabularyName,
            DisplayNameArabic = vocabulary.DisplayNameArabic,
            DisplayNameEnglish = vocabulary.DisplayNameEnglish,
            Description = vocabulary.Description,
            Version = vocabulary.Version,
            Category = vocabulary.Category,
            IsActive = vocabulary.IsActive,
            ValueCount = vocabulary.ValueCount,
            Values = ParseValues(vocabulary.ValuesJson)
        };

        return dto;
    }

    private static List<VocabularyValueDto> ParseValues(string valuesJson)
    {
        if (string.IsNullOrWhiteSpace(valuesJson) || valuesJson == "[]")
            return new List<VocabularyValueDto>();

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var rawValues = JsonSerializer.Deserialize<List<VocabularyRawValue>>(valuesJson, options);

            if (rawValues == null)
                return new List<VocabularyValueDto>();

            return rawValues.Select(v => new VocabularyValueDto
            {
                Code = v.Code,
                LabelArabic = v.LabelAr ?? string.Empty,
                LabelEnglish = v.LabelEn ?? string.Empty,
                Description = v.Description,
                DisplayOrder = v.DisplayOrder
            }).ToList();
        }
        catch
        {
            return new List<VocabularyValueDto>();
        }
    }

    /// <summary>
    /// Internal model matching the JSON structure stored in Vocabulary.ValuesJson
    /// </summary>
    private class VocabularyRawValue
    {
        public int Code { get; set; }
        public string? LabelAr { get; set; }
        public string? LabelEn { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}
