using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Mappings;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Vocabularies.Commands.CreateVocabulary;

public class CreateVocabularyCommandHandler : IRequestHandler<CreateVocabularyCommand, VocabularyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IVocabularyValidationService _vocabService;
    private readonly ILogger<CreateVocabularyCommandHandler> _logger;

    public CreateVocabularyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IVocabularyValidationService vocabService,
        ILogger<CreateVocabularyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _vocabService = vocabService ?? throw new ArgumentNullException(nameof(vocabService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VocabularyDto> Handle(CreateVocabularyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Check for duplicate vocabulary name
        var exists = await _unitOfWork.Vocabularies.ExistsAsync(request.VocabularyName, cancellationToken);
        if (exists)
            throw new ValidationException($"A vocabulary with the name '{request.VocabularyName}' already exists.");

        // Serialize values to JSON for domain entity
        var valuesJson = SerializeValues(request.Values);

        var vocabulary = Vocabulary.Create(
            vocabularyName: request.VocabularyName,
            displayNameArabic: request.DisplayNameArabic,
            displayNameEnglish: request.DisplayNameEnglish,
            description: request.Description,
            valuesJson: valuesJson,
            isSystemVocabulary: request.IsSystemVocabulary,
            allowCustomValues: request.AllowCustomValues,
            category: request.Category,
            createdByUserId: currentUserId);

        await _unitOfWork.Vocabularies.AddAsync(vocabulary, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _vocabService.InvalidateCache();

        _logger.LogInformation("Vocabulary '{VocabularyName}' created with ID {VocabularyId}",
            vocabulary.VocabularyName, vocabulary.Id);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created vocabulary '{vocabulary.VocabularyName}' (v{vocabulary.Version})",
            entityType: "Vocabulary",
            entityId: vocabulary.Id,
            entityIdentifier: vocabulary.VocabularyName,
            newValues: JsonSerializer.Serialize(new
            {
                vocabulary.VocabularyName,
                vocabulary.DisplayNameArabic,
                vocabulary.DisplayNameEnglish,
                vocabulary.Category,
                vocabulary.Version,
                vocabulary.ValueCount,
                vocabulary.IsSystemVocabulary
            }),
            cancellationToken: cancellationToken);

        return VocabularyMappingHelper.MapToDto(vocabulary);
    }

    private static string SerializeValues(List<VocabularyValueDto> values)
    {
        var rawValues = values.Select(v => new
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
