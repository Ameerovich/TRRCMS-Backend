using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Mappings;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Vocabularies.Commands.UpdateVocabularyMetadata;

public class UpdateVocabularyMetadataCommandHandler : IRequestHandler<UpdateVocabularyMetadataCommand, VocabularyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ILogger<UpdateVocabularyMetadataCommandHandler> _logger;

    public UpdateVocabularyMetadataCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ILogger<UpdateVocabularyMetadataCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VocabularyDto> Handle(UpdateVocabularyMetadataCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var vocabulary = await _unitOfWork.Vocabularies.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Vocabulary with ID '{request.Id}' not found.");

        // Capture old values for audit
        var oldValues = new
        {
            vocabulary.DisplayNameArabic,
            vocabulary.DisplayNameEnglish,
            vocabulary.Description,
            vocabulary.Category
        };

        vocabulary.UpdateMetadata(
            displayNameArabic: request.DisplayNameArabic,
            displayNameEnglish: request.DisplayNameEnglish,
            description: request.Description,
            category: request.Category,
            modifiedByUserId: currentUserId);

        await _unitOfWork.Vocabularies.UpdateAsync(vocabulary, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vocabulary '{VocabularyName}' metadata updated by user {UserId}",
            vocabulary.VocabularyName, currentUserId);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated metadata for vocabulary '{vocabulary.VocabularyName}'",
            entityType: "Vocabulary",
            entityId: vocabulary.Id,
            entityIdentifier: vocabulary.VocabularyName,
            oldValues: JsonSerializer.Serialize(oldValues),
            newValues: JsonSerializer.Serialize(new
            {
                vocabulary.DisplayNameArabic,
                vocabulary.DisplayNameEnglish,
                vocabulary.Description,
                vocabulary.Category
            }),
            changedFields: "DisplayNameArabic, DisplayNameEnglish, Description, Category",
            cancellationToken: cancellationToken);

        return VocabularyMappingHelper.MapToDto(vocabulary);
    }
}
