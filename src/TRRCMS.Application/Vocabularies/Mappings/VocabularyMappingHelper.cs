using System.Text.Json;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Vocabularies.Mappings;

/// <summary>
/// Shared mapping helper for Vocabulary entity → VocabularyDto conversion.
/// Handles JSON deserialization of ValuesJson for all vocabulary query/command handlers.
/// </summary>
public static class VocabularyMappingHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static VocabularyDto MapToDto(Vocabulary vocabulary)
    {
        return new VocabularyDto
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
    }

    public static List<VocabularyDto> MapToDtoList(IEnumerable<Vocabulary> vocabularies)
    {
        return vocabularies.Select(MapToDto).ToList();
    }

    private static List<VocabularyValueDto> ParseValues(string valuesJson)
    {
        if (string.IsNullOrWhiteSpace(valuesJson) || valuesJson == "[]")
            return new List<VocabularyValueDto>();

        try
        {
            var rawValues = JsonSerializer.Deserialize<List<VocabularyRawValue>>(valuesJson, JsonOptions);

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

    private class VocabularyRawValue
    {
        public int Code { get; set; }
        public string? LabelAr { get; set; }
        public string? LabelEn { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}
