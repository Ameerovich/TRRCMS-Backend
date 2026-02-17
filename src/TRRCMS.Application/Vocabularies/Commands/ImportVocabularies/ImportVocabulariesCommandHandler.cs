using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Vocabularies.Commands.ImportVocabularies;

public class ImportVocabulariesCommandHandler : IRequestHandler<ImportVocabulariesCommand, ImportVocabulariesResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IVocabularyValidationService _vocabService;
    private readonly ILogger<ImportVocabulariesCommandHandler> _logger;

    public ImportVocabulariesCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IVocabularyValidationService vocabService,
        ILogger<ImportVocabulariesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _vocabService = vocabService ?? throw new ArgumentNullException(nameof(vocabService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ImportVocabulariesResult> Handle(ImportVocabulariesCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var result = new ImportVocabulariesResult();

        foreach (var importVocab in request.Vocabularies)
        {
            var existing = await _unitOfWork.Vocabularies.GetByNameAsync(importVocab.VocabularyName, currentOnly: true, cancellationToken);

            var valuesJson = SerializeValues(importVocab);

            if (existing == null)
            {
                // Create new vocabulary
                var vocabulary = Vocabulary.Create(
                    vocabularyName: importVocab.VocabularyName,
                    displayNameArabic: importVocab.DisplayNameArabic,
                    displayNameEnglish: importVocab.DisplayNameEnglish,
                    description: importVocab.Description,
                    valuesJson: valuesJson,
                    isSystemVocabulary: importVocab.IsSystemVocabulary,
                    allowCustomValues: importVocab.AllowCustomValues,
                    category: importVocab.Category,
                    createdByUserId: currentUserId);

                await _unitOfWork.Vocabularies.AddAsync(vocabulary, cancellationToken);
                result.Created++;
                result.Messages.Add($"Created vocabulary '{importVocab.VocabularyName}'");
            }
            else
            {
                // Create new version with imported values
                var newVersion = existing.CreateMinorVersion(
                    valuesJson,
                    $"Imported from snapshot (was v{existing.Version})",
                    currentUserId);

                await _unitOfWork.Vocabularies.UpdateAsync(existing, cancellationToken);
                await _unitOfWork.Vocabularies.AddAsync(newVersion, cancellationToken);
                result.Updated++;
                result.Messages.Add($"Updated vocabulary '{importVocab.VocabularyName}' from v{existing.Version} to v{newVersion.Version}");
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _vocabService.InvalidateCache();

        _logger.LogInformation(
            "Vocabulary import completed: {Created} created, {Updated} updated, {Skipped} skipped",
            result.Created, result.Updated, result.Skipped);

        return result;
    }

    private static string SerializeValues(Queries.ExportVocabularies.VocabularyExportDto vocab)
    {
        var rawValues = vocab.Values.Select(v => new
        {
            code = v.Code,
            labelAr = v.LabelArabic,
            labelEn = v.LabelEnglish,
            description = v.Description,
            displayOrder = v.DisplayOrder
        });

        return JsonSerializer.Serialize(rawValues);
    }
}
