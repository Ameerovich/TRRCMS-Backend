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
            throw new ValidationException(
                "Can only create new versions from the current version of a vocabulary.",
                "Message_Vocabulary_NotCurrentVersion");

        if (vocabulary.IsSystemVocabulary && request.VersionType.ToLowerInvariant() == "major")
            throw new ValidationException(
                $"System vocabulary '{vocabulary.VocabularyName}' cannot have new codes added. " +
                "Its values are fixed by the domain model. " +
                "Only 'patch' (label fixes) and 'minor' (deprecations) versions are permitted.",
                "Message_Vocabulary_SystemMajorBlocked",
                vocabulary.VocabularyName);

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

            var removedJoined = string.Join(", ", removedLabels);
            throw new ValidationException(
                $"Vocabulary codes cannot be removed. The following codes are missing from the new version: " +
                $"{removedJoined}. " +
                $"Set isDeprecated=true on these codes instead to deprecate them.",
                "Message_Vocabulary_CodesCannotBeRemoved",
                removedJoined);
        }

        // Block deprecations on patch versions — patch is label-fixes only
        if (request.VersionType.ToLowerInvariant() == "patch")
        {
            var oldDeprecatedCodes = new HashSet<int>(
                oldValues.Where(v => v.IsDeprecated).Select(v => v.Code));

            var newlyDeprecated = request.Values
                .Where(v => v.IsDeprecated && !oldDeprecatedCodes.Contains(v.Code))
                .ToList();

            if (newlyDeprecated.Any())
            {
                var joined = string.Join(", ", newlyDeprecated
                    .Select(v => $"{v.Code} ({v.LabelArabic} / {v.LabelEnglish})"));

                throw new ValidationException(
                    $"Deprecating codes is not allowed in a patch version. " +
                    $"The following codes were newly deprecated: {joined}. " +
                    $"Use a minor version to deprecate codes.",
                    "Message_Vocabulary_PatchCannotDeprecate",
                    joined);
            }
        }

        // Enforce AllowCustomValues — if false, block new codes on minor/patch versions
        if (!vocabulary.AllowCustomValues && request.VersionType.ToLowerInvariant() != "major")
        {
            var addedCodes = newCodes.Except(oldCodes).ToList();
            if (addedCodes.Any())
            {
                var addedJoined = string.Join(", ", addedCodes);
                throw new ValidationException(
                    $"Adding new codes requires a major version. " +
                    $"The new codes ({addedJoined}) cannot be added in a {request.VersionType} version — " +
                    $"only label edits and deprecations are permitted there. " +
                    $"Change the version type to 'major' to add the new codes.",
                    "Message_Vocabulary_CustomValuesBlocked",
                    addedJoined, request.VersionType);
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
            _ => throw new ValidationException(
                $"Invalid version type '{request.VersionType}'. Must be 'minor', 'major', or 'patch'.",
                "Message_Vocabulary_InvalidVersionType",
                request.VersionType)
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
