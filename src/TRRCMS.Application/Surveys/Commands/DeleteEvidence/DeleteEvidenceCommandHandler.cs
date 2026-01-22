using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.DeleteEvidence;

/// <summary>
/// Handler for DeleteEvidenceCommand
/// Performs soft delete and removes physical file
/// </summary>
public class DeleteEvidenceCommandHandler : IRequestHandler<DeleteEvidenceCommand, Unit>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeleteEvidenceCommandHandler(
        ISurveyRepository surveyRepository,
        IEvidenceRepository evidenceRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Unit> Handle(DeleteEvidenceCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get and validate survey
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership
        if (survey.FieldCollectorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only delete evidence for your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot delete evidence for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Get evidence
        var evidence = await _evidenceRepository.GetByIdAsync(request.EvidenceId, cancellationToken);
        if (evidence == null)
        {
            throw new NotFoundException($"Evidence with ID {request.EvidenceId} not found");
        }

        // Store info for audit log before deletion
        var evidenceInfo = new
        {
            evidence.Id,
            evidence.EvidenceType,
            evidence.OriginalFileName,
            evidence.FilePath,
            evidence.Description
        };

        // Delete physical file
        try
        {
            await _fileStorageService.DeleteFileAsync(evidence.FilePath, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but don't fail - file might already be deleted
            // In production, use ILogger here
            Console.WriteLine($"Warning: Could not delete physical file: {ex.Message}");
        }

        // Soft delete evidence
        evidence.MarkAsDeleted(currentUserId);
        await _evidenceRepository.UpdateAsync(evidence, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Delete,
            actionDescription: $"Deleted evidence '{evidenceInfo.OriginalFileName}' (Type: {evidenceInfo.EvidenceType}) from survey {survey.ReferenceCode}",
            entityType: "Evidence",
            entityId: evidence.Id,
            entityIdentifier: evidenceInfo.OriginalFileName,
            oldValues: System.Text.Json.JsonSerializer.Serialize(evidenceInfo),
            newValues: null,
            changedFields: "Deleted",
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}