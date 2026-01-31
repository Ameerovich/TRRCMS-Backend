using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.DeletePersonPropertyRelation;

public class DeletePersonPropertyRelationCommandHandler : IRequestHandler<DeletePersonPropertyRelationCommand, Unit>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeletePersonPropertyRelationCommandHandler(
        ISurveyRepository surveyRepository,
        IPersonPropertyRelationRepository relationRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IEvidenceRepository evidenceRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _surveyRepository = surveyRepository;
        _relationRepository = relationRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _evidenceRepository = evidenceRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(DeletePersonPropertyRelationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get and validate survey
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only delete relations in your own surveys");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot modify survey in {survey.Status} status");

        // Get and validate relation with evidences
        var relation = await _relationRepository.GetByIdWithEvidencesAsync(request.RelationId, cancellationToken)
            ?? throw new NotFoundException($"Relation with ID {request.RelationId} not found");

        // Verify relation belongs to survey's building
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(relation.PropertyUnitId, cancellationToken)
            ?? throw new NotFoundException($"Property unit not found");

        if (propertyUnit.BuildingId != survey.BuildingId)
            throw new ValidationException("Relation does not belong to this survey's building");

        // Capture info for audit
        var relationInfo = new
        {
            relation.Id,
            relation.PersonId,
            relation.PropertyUnitId,
            RelationType = relation.RelationType.ToString(),
            ContractType = relation.ContractType?.ToString(),
            relation.OwnershipShare,
            EvidenceCount = relation.Evidences?.Count ?? 0
        };

        // Delete associated evidence files and records
        if (relation.Evidences != null && relation.Evidences.Any())
        {
            foreach (var evidence in relation.Evidences.ToList())
            {
                // Delete file from storage if requested
                if (request.DeleteEvidenceFiles && !string.IsNullOrEmpty(evidence.FilePath))
                {
                    try
                    {
                        await _fileStorageService.DeleteFileAsync(evidence.FilePath, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // Log but continue - the evidence record will still be deleted
                        Console.WriteLine($"Warning: Failed to delete evidence file {evidence.FilePath}: {ex.Message}");
                    }
                }

                // Soft delete evidence record
                await _evidenceRepository.DeleteAsync(evidence, cancellationToken);
            }
        }

        // Soft delete the relation
        await _relationRepository.DeleteAsync(relation, cancellationToken);
        await _relationRepository.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogActionAsync(
            AuditActionType.Delete,
            $"Deleted relation ({relation.RelationType}) for property unit {propertyUnit.UnitIdentifier}",
            "PersonPropertyRelation",
            relation.Id,
            relation.Id.ToString(),
            System.Text.Json.JsonSerializer.Serialize(relationInfo),
            null,
            "Deleted",
            cancellationToken);

        return Unit.Value;
    }
}
