using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Mappings;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Vocabularies.Commands.CreateVocabularyVersion;

public class CreateVocabularyVersionCommandHandler : IRequestHandler<CreateVocabularyVersionCommand, VocabularyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IVocabularyValidationService _vocabService;
    private readonly ILogger<CreateVocabularyVersionCommandHandler> _logger;

    public CreateVocabularyVersionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IVocabularyValidationService vocabService,
        ILogger<CreateVocabularyVersionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _vocabService = vocabService ?? throw new ArgumentNullException(nameof(vocabService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VocabularyDto> Handle(CreateVocabularyVersionCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var vocabulary = await _unitOfWork.Vocabularies.GetByIdAsync(request.VocabularyId, cancellationToken)
            ?? throw new NotFoundException($"Vocabulary with ID '{request.VocabularyId}' not found.");

        if (!vocabulary.IsCurrentVersion)
            throw new ValidationException("Can only create new versions from the current version of a vocabulary.");

        // Block code removal — codes can only be deprecated, never removed
        var oldValues = VocabularyMappingHelper.ParseValues(vocabulary.ValuesJson);
        var oldCodes = new HashSet<int>(oldValues.Select(v => v.Code));
        var newCodes = new HashSet<int>(request.Values.Select(v => v.Code));
        var removedCodes = oldCodes.Except(newCodes).ToList();

        if (removedCodes.Any())
        {
            var removedLabels = oldValues
                .Where(v => removedCodes.Contains(v.Code))
                .Select(v => $"{v.Code} ({v.LabelArabic} / {v.LabelEnglish})")
                .ToList();

            throw new ValidationException(
                $"Vocabulary codes cannot be removed. The following codes are missing from the new version: " +
                $"{string.Join(", ", removedLabels)}. " +
                $"Set isDeprecated=true on these codes instead to deprecate them.");
        }

        // Enforce AllowCustomValues — if false, block new codes on minor/patch versions
        if (!vocabulary.AllowCustomValues && request.VersionType.ToLowerInvariant() != "major")
        {
            var addedCodes = newCodes.Except(oldCodes).ToList();
            if (addedCodes.Any())
            {
                throw new ValidationException(
                    $"This vocabulary does not allow custom values. " +
                    $"New codes ({string.Join(", ", addedCodes)}) cannot be added in a {request.VersionType} version. " +
                    $"Only label changes and deprecation are permitted. Use a major version to add new codes.");
            }
        }

        var valuesJson = SerializeValues(request.Values);
        var oldVersion = vocabulary.Version;

        // Domain method marks the old entity as non-current and returns a new entity
        Vocabulary newVersion = request.VersionType.ToLowerInvariant() switch
        {
            "minor" => vocabulary.CreateMinorVersion(valuesJson, request.ChangeLog, currentUserId),
            "major" => vocabulary.CreateMajorVersion(valuesJson, request.ChangeLog, currentUserId),
            "patch" => vocabulary.CreatePatchVersion(valuesJson, request.ChangeLog, currentUserId),
            _ => throw new ValidationException($"Invalid version type '{request.VersionType}'. Must be 'minor', 'major', or 'patch'.")
        };

        // Persist: update old (IsCurrentVersion = false), add new
        await _unitOfWork.Vocabularies.UpdateAsync(vocabulary, cancellationToken);
        await _unitOfWork.Vocabularies.AddAsync(newVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _vocabService.InvalidateCache();

        _logger.LogInformation(
            "Vocabulary '{VocabularyName}' versioned from {OldVersion} to {NewVersion} ({VersionType})",
            vocabulary.VocabularyName, oldVersion, newVersion.Version, request.VersionType);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.VocabularyUpdate,
            actionDescription: $"Created {request.VersionType} version {newVersion.Version} for vocabulary '{vocabulary.VocabularyName}' (was {oldVersion})",
            entityType: "Vocabulary",
            entityId: newVersion.Id,
            entityIdentifier: vocabulary.VocabularyName,
            oldValues: JsonSerializer.Serialize(new { Version = oldVersion, vocabulary.ValueCount }),
            newValues: JsonSerializer.Serialize(new { newVersion.Version, newVersion.ValueCount, request.ChangeLog }),
            changedFields: "Version, ValuesJson, ValueCount",
            cancellationToken: cancellationToken);

        return VocabularyMappingHelper.MapToDto(newVersion);
    }

    private static string SerializeValues(List<VocabularyValueDto> values)
    {
        var rawValues = values.Select(v => new
        {
            code = v.Code,
            labelAr = v.LabelArabic,
            labelEn = v.LabelEnglish,
            description = v.Description,
            displayOrder = v.DisplayOrder,
            isDeprecated = v.IsDeprecated
        });

        return JsonSerializer.Serialize(rawValues);
    }
}
