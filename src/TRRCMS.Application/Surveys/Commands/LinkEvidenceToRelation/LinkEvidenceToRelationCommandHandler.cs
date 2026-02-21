using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.LinkEvidenceToRelation;

/// <summary>
/// Handler for LinkEvidenceToRelationCommand.
/// Links an existing Evidence to a PersonPropertyRelation via EvidenceRelation join entity.
/// </summary>
public class LinkEvidenceToRelationCommandHandler : IRequestHandler<LinkEvidenceToRelationCommand, EvidenceDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IEvidenceRelationRepository _evidenceRelationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public LinkEvidenceToRelationCommandHandler(
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

    public async Task<EvidenceDto> Handle(LinkEvidenceToRelationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // 1. Validate survey exists and user owns it
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only link evidence for your own surveys");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot link evidence for survey in {survey.Status} status. Only Draft surveys can be modified.");

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

        // 5. Check for duplicate active link
        var existingLink = await _evidenceRelationRepository.LinkExistsAsync(
            request.EvidenceId, request.PersonPropertyRelationId, cancellationToken);

        if (existingLink)
            throw new ValidationException("This evidence is already linked to the specified person-property relation");

        // 6. Create EvidenceRelation join entity
        var evidenceRelation = EvidenceRelation.Create(
            evidenceId: request.EvidenceId,
            personPropertyRelationId: request.PersonPropertyRelationId,
            linkedBy: currentUserId,
            linkReason: request.LinkReason);

        await _evidenceRelationRepository.AddAsync(evidenceRelation, cancellationToken);

        // 7. Update HasEvidence flag on the relation
        relation.SetHasEvidence(true, currentUserId);

        await _evidenceRelationRepository.SaveChangesAsync(cancellationToken);

        // 8. Audit log
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Linked evidence '{evidence.OriginalFileName}' to person-property relation in survey {survey.ReferenceCode}",
            entityType: "EvidenceRelation",
            entityId: evidenceRelation.Id,
            entityIdentifier: $"{request.EvidenceId} → {request.PersonPropertyRelationId}",
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                EvidenceId = request.EvidenceId,
                PersonPropertyRelationId = request.PersonPropertyRelationId,
                request.LinkReason,
                EvidenceType = evidence.EvidenceType.ToString(),
                RelationType = relation.RelationType.ToString(),
                SurveyId = request.SurveyId,
                SurveyReferenceCode = survey.ReferenceCode
            }),
            changedFields: "New Evidence-Relation Link",
            cancellationToken: cancellationToken);

        // Re-fetch evidence with all EvidenceRelations included
        var updatedEvidence = await _evidenceRepository.GetByIdAsync(request.EvidenceId, cancellationToken);
        var result = _mapper.Map<EvidenceDto>(updatedEvidence!);
        result.IsExpired = updatedEvidence!.IsExpired();
        return result;
    }
}
