using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UnlinkEvidenceFromRelation;

/// <summary>
/// Handler for UnlinkEvidenceFromRelationCommand.
/// Deactivates the EvidenceRelation join row between an Evidence and a single
/// PersonPropertyRelation, leaving the Evidence and any other links intact.
/// </summary>
public class UnlinkEvidenceFromRelationCommandHandler : IRequestHandler<UnlinkEvidenceFromRelationCommand, EvidenceDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IEvidenceRelationRepository _evidenceRelationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UnlinkEvidenceFromRelationCommandHandler(
        ISurveyRepository surveyRepository,
        IEvidenceRepository evidenceRepository,
        IPersonPropertyRelationRepository relationRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IEvidenceRelationRepository evidenceRelationRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _relationRepository = relationRepository ?? throw new ArgumentNullException(nameof(relationRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _evidenceRelationRepository = evidenceRelationRepository ?? throw new ArgumentNullException(nameof(evidenceRelationRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<EvidenceDto> Handle(UnlinkEvidenceFromRelationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // 1. Validate survey exists and user owns it (or has edit-all permission)
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_EditAll))
                throw new UnauthorizedAccessException("You can only unlink evidence for your own surveys");
        }

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot unlink evidence for survey in {survey.Status} status. Only Draft surveys can be modified.");

        // 2. Validate evidence exists
        var evidence = await _evidenceRepository.GetByIdAsync(request.EvidenceId, cancellationToken)
            ?? throw new NotFoundException($"Evidence with ID {request.EvidenceId} not found");

        // 3. Validate relation exists
        var relation = await _relationRepository.GetByIdAsync(request.PersonPropertyRelationId, cancellationToken)
            ?? throw new NotFoundException($"Person-property relation with ID {request.PersonPropertyRelationId} not found");

        // 4. Validate relation belongs to the survey's building context
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(relation.PropertyUnitId, cancellationToken)
            ?? throw new NotFoundException($"Property unit with ID {relation.PropertyUnitId} not found");

        if (propertyUnit.BuildingId != survey.BuildingId)
            throw new ValidationException("Person-property relation does not belong to the survey's building");

        // 5. Locate the link to remove
        var link = await _evidenceRelationRepository.GetByEvidenceAndRelationAsync(
            request.EvidenceId, request.PersonPropertyRelationId, cancellationToken)
            ?? throw new NotFoundException("This evidence is not linked to the specified person-property relation");

        if (!link.IsActive)
            throw new ValidationException("This evidence is already unlinked from the specified person-property relation");

        // 6. Recompute the relation's HasEvidence flag. Evaluate the currently-active links
        //    BEFORE deactivating (the deactivation isn't persisted yet, so a query would still
        //    count this row). If this link is the relation's only active evidence link, the
        //    relation will no longer have any evidence once it is removed.
        var activeLinks = await _evidenceRelationRepository.GetActiveByRelationIdAsync(
            request.PersonPropertyRelationId, cancellationToken);
        var relationHasOtherEvidence = activeLinks.Any(l => l.Id != link.Id);

        // 7. Deactivate the link (soft unlink, reversible, audit reason preserved)
        link.Deactivate(currentUserId, request.Reason ?? "Unlinked by user");
        await _evidenceRelationRepository.UpdateAsync(link, cancellationToken);

        if (!relationHasOtherEvidence)
            relation.SetHasEvidence(false, currentUserId);

        await _evidenceRelationRepository.SaveChangesAsync(cancellationToken);

        // 8. Audit log
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Delete,
            actionDescription: $"Unlinked evidence '{evidence.OriginalFileName}' from person-property relation in survey {survey.ReferenceCode}",
            entityType: "EvidenceRelation",
            entityId: link.Id,
            entityIdentifier: $"{request.EvidenceId} → {request.PersonPropertyRelationId}",
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                EvidenceId = request.EvidenceId,
                PersonPropertyRelationId = request.PersonPropertyRelationId,
                request.Reason,
                EvidenceType = evidence.EvidenceType.ToString(),
                RelationType = relation.RelationType.ToString(),
                RelationStillHasEvidence = relationHasOtherEvidence,
                SurveyId = request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode
            }),
            changedFields: "Evidence-Relation Link Deactivated",
            cancellationToken: cancellationToken);

        // Re-fetch evidence with its remaining relations included
        var updatedEvidence = await _evidenceRepository.GetByIdAsync(request.EvidenceId, cancellationToken);
        var result = _mapper.Map<EvidenceDto>(updatedEvidence!);
        result.IsExpired = updatedEvidence!.IsExpired();
        return result;
    }
}
