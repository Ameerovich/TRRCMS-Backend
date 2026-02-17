using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Mappings;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Vocabularies.Commands.DeactivateVocabulary;

public class DeactivateVocabularyCommandHandler : IRequestHandler<DeactivateVocabularyCommand, VocabularyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IVocabularyValidationService _vocabService;
    private readonly ILogger<DeactivateVocabularyCommandHandler> _logger;

    public DeactivateVocabularyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IVocabularyValidationService vocabService,
        ILogger<DeactivateVocabularyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _vocabService = vocabService ?? throw new ArgumentNullException(nameof(vocabService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VocabularyDto> Handle(DeactivateVocabularyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var vocabulary = await _unitOfWork.Vocabularies.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Vocabulary with ID '{request.Id}' not found.");

        if (vocabulary.IsSystemVocabulary)
            throw new ValidationException($"System vocabulary '{vocabulary.VocabularyName}' cannot be deactivated.");

        if (!vocabulary.IsActive)
            throw new ValidationException($"Vocabulary '{vocabulary.VocabularyName}' is already inactive.");

        vocabulary.Deactivate(currentUserId);

        await _unitOfWork.Vocabularies.UpdateAsync(vocabulary, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _vocabService.InvalidateCache();

        _logger.LogInformation("Vocabulary '{VocabularyName}' deactivated by user {UserId}",
            vocabulary.VocabularyName, currentUserId);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.StatusChange,
            actionDescription: $"Deactivated vocabulary '{vocabulary.VocabularyName}'",
            entityType: "Vocabulary",
            entityId: vocabulary.Id,
            entityIdentifier: vocabulary.VocabularyName,
            oldValues: JsonSerializer.Serialize(new { IsActive = true }),
            newValues: JsonSerializer.Serialize(new { IsActive = false }),
            changedFields: "IsActive",
            cancellationToken: cancellationToken);

        return VocabularyMappingHelper.MapToDto(vocabulary);
    }
}
