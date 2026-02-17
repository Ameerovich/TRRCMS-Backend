using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Mappings;

namespace TRRCMS.Application.Vocabularies.Queries.ExportVocabularies;

public class ExportVocabulariesQueryHandler : IRequestHandler<ExportVocabulariesQuery, List<VocabularyExportDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ExportVocabulariesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<List<VocabularyExportDto>> Handle(ExportVocabulariesQuery request, CancellationToken cancellationToken)
    {
        var vocabularies = await _unitOfWork.Vocabularies.GetAllCurrentAsync(cancellationToken);

        return vocabularies.Select(v =>
        {
            var dto = VocabularyMappingHelper.MapToDto(v);
            return new VocabularyExportDto
            {
                VocabularyName = dto.VocabularyName,
                DisplayNameArabic = dto.DisplayNameArabic,
                DisplayNameEnglish = dto.DisplayNameEnglish,
                Description = dto.Description,
                Version = dto.Version,
                Category = dto.Category,
                IsActive = dto.IsActive,
                IsSystemVocabulary = v.IsSystemVocabulary,
                AllowCustomValues = v.AllowCustomValues,
                Values = dto.Values
            };
        }).ToList();
    }
}
