using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.DeletePersonPropertyRelation;

public class DeletePersonPropertyRelationCommandHandler : IRequestHandler<DeletePersonPropertyRelationCommand, DeleteResultDto>
{
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeletePersonPropertyRelationCommandHandler(
        IPersonPropertyRelationRepository relationRepository,
        IEvidenceRepository evidenceRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _relationRepository = relationRepository;
        _evidenceRepository = evidenceRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _surveyRepository = surveyRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<DeleteResultDto> Handle(DeletePersonPropertyRelationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var affectedEntities = new List<DeletedEntityInfo>();

        // Get relation
        var relation = await _relationRepository.GetByIdWithEvidencesAsync(request.RelationId, cancellationToken)
            ?? throw new NotFoundException($"PersonPropertyRelation with ID {request.RelationId} not found");

        if (relation.IsDeleted)
            throw new ValidationException("PersonPropertyRelation is already deleted");

        // Get property unit and validate survey status
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(relation.PropertyUnitId, cancellationToken)
            ?? throw new NotFoundException("Property unit not found");

        var surveys = await _surveyRepository.GetByBuildingAsync(propertyUnit.BuildingId, cancellationToken);
        var survey = surveys.FirstOrDefault(s => !s.IsDeleted);

        if (survey != null && survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot delete relation. Survey status is {survey.Status}. Only surveys in Draft status can be modified.");

        // CASCADE DELETE: Delete all related evidences
        var evidences = await _evidenceRepository.GetByRelationIdAsync(request.RelationId, cancellationToken);
        foreach (var evidence in evidences.Where(e => !e.IsDeleted))
        {
            evidence.MarkAsDeleted(currentUserId);
            await _evidenceRepository.UpdateAsync(evidence, cancellationToken);

            affectedEntities.Add(new DeletedEntityInfo
            {
                EntityId = evidence.Id,
                EntityType = "Evidence",
                EntityIdentifier = evidence.OriginalFileName
            });
        }

        // Soft delete relation
        relation.MarkAsDeleted(currentUserId);
        await _relationRepository.UpdateAsync(relation, cancellationToken);

        affectedEntities.Insert(0, new DeletedEntityInfo
        {
            EntityId = relation.Id,
            EntityType = "PersonPropertyRelation",
            EntityIdentifier = $"Relation {relation.RelationType}"
        });

        await _relationRepository.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogActionAsync(
            AuditActionType.Delete,
            $"Deleted PersonPropertyRelation and {evidences.Count()} evidences",
            "PersonPropertyRelation",
            relation.Id,
            relation.Id.ToString(),
            null,
            null,
            $"Cascaded to {evidences.Count()} evidences",
            cancellationToken);

        return new DeleteResultDto
        {
            PrimaryEntityId = relation.Id,
            PrimaryEntityType = "PersonPropertyRelation",
            AffectedEntities = affectedEntities,
            TotalAffected = affectedEntities.Count,
            DeletedAtUtc = relation.DeletedAtUtc!.Value,
            Message = $"PersonPropertyRelation deleted successfully along with {evidences.Count()} evidence(s)"
        };
    }
}
