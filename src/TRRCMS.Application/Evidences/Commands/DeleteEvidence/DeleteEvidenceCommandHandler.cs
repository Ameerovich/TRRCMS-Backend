using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Evidences.Commands.DeleteEvidence;

public class DeleteEvidenceCommandHandler : IRequestHandler<DeleteEvidenceCommand, DeleteResultDto>
{
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeleteEvidenceCommandHandler(
        IEvidenceRepository evidenceRepository,
        IPersonPropertyRelationRepository relationRepository,
        IPersonRepository personRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _evidenceRepository = evidenceRepository;
        _relationRepository = relationRepository;
        _personRepository = personRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _surveyRepository = surveyRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<DeleteResultDto> Handle(DeleteEvidenceCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get evidence
        var evidence = await _evidenceRepository.GetByIdAsync(request.EvidenceId, cancellationToken)
            ?? throw new NotFoundException($"Evidence with ID {request.EvidenceId} not found");

        if (evidence.IsDeleted)
            throw new ValidationException("Evidence is already deleted");

        // Find related survey to validate status
        var survey = await FindRelatedSurveyAsync(evidence, cancellationToken);
        if (survey == null)
            throw new ValidationException("Cannot find related survey for this evidence");

        // Validate survey status
        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot delete evidence. Survey status is {survey.Status}. Only surveys in Draft status can be modified.");

        // Soft delete evidence
        evidence.MarkAsDeleted(currentUserId);
        await _evidenceRepository.UpdateAsync(evidence, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogActionAsync(
            AuditActionType.Delete,
            $"Deleted evidence: {evidence.OriginalFileName}",
            "Evidence",
            evidence.Id,
            evidence.OriginalFileName,
            null,
            null,
            "Soft deleted",
            cancellationToken);

        return new DeleteResultDto
        {
            PrimaryEntityId = evidence.Id,
            PrimaryEntityType = "Evidence",
            AffectedEntities = new List<DeletedEntityInfo>
            {
                new()
                {
                    EntityId = evidence.Id,
                    EntityType = "Evidence",
                    EntityIdentifier = evidence.OriginalFileName
                }
            },
            TotalAffected = 1,
            DeletedAtUtc = evidence.DeletedAtUtc!.Value,
            Message = "Evidence deleted successfully"
        };
    }

    private async Task<Domain.Entities.Survey?> FindRelatedSurveyAsync(
        Domain.Entities.Evidence evidence,
        CancellationToken cancellationToken)
    {
        // Try to find survey through PersonPropertyRelation
        if (evidence.PersonPropertyRelationId.HasValue)
        {
            var relation = await _relationRepository.GetByIdAsync(
                evidence.PersonPropertyRelationId.Value, cancellationToken);
            if (relation != null)
            {
                var propertyUnit = await _propertyUnitRepository.GetByIdAsync(
                    relation.PropertyUnitId, cancellationToken);
                if (propertyUnit != null)
                {
                    var surveys = await _surveyRepository.GetByBuildingAsync(
                        propertyUnit.BuildingId, cancellationToken);
                    return surveys.FirstOrDefault();
                }
            }
        }

        // Try to find survey through Person -> Household -> PropertyUnit
        if (evidence.PersonId.HasValue)
        {
            var person = await _personRepository.GetByIdAsync(
                evidence.PersonId.Value, cancellationToken);
            if (person?.HouseholdId != null)
            {
                // This requires a household repository method - for now assume we can navigate
                // In a real implementation, you'd inject IHouseholdRepository
                // and get the household to access PropertyUnitId
            }
        }

        return null;
    }
}
