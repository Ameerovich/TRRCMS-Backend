using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdatePersonPropertyRelation;

public class UpdatePersonPropertyRelationCommandHandler : IRequestHandler<UpdatePersonPropertyRelationCommand, PersonPropertyRelationDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdatePersonPropertyRelationCommandHandler(
        ISurveyRepository surveyRepository,
        IPersonPropertyRelationRepository relationRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IPersonRepository personRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _surveyRepository = surveyRepository;
        _relationRepository = relationRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _personRepository = personRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<PersonPropertyRelationDto> Handle(UpdatePersonPropertyRelationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get and validate survey
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only update relations in your own surveys");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot modify survey in {survey.Status} status");

        // Get and validate relation
        var relation = await _relationRepository.GetByIdWithEvidencesAsync(request.RelationId, cancellationToken)
            ?? throw new NotFoundException($"Relation with ID {request.RelationId} not found");

        // Verify relation belongs to survey's building
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(relation.PropertyUnitId, cancellationToken)
            ?? throw new NotFoundException($"Property unit not found");

        if (propertyUnit.BuildingId != survey.BuildingId)
            throw new ValidationException("Relation does not belong to this survey's building");

        // Capture old values for audit
        var oldValues = new
        {
            relation.PersonId,
            relation.PropertyUnitId,
            RelationType = relation.RelationType.ToString(),
            OccupancyType = relation.OccupancyType?.ToString(),
            relation.HasEvidence,
            relation.OwnershipShare,
            relation.ContractDetails,
            relation.Notes
        };

        // Update PersonId if provided
        if (request.PersonId.HasValue && request.PersonId.Value != relation.PersonId)
        {
            var person = await _personRepository.GetByIdAsync(request.PersonId.Value, cancellationToken)
                ?? throw new NotFoundException($"Person with ID {request.PersonId} not found");
            relation.UpdatePersonId(request.PersonId.Value, currentUserId);
        }

        // Update PropertyUnitId if provided
        if (request.PropertyUnitId.HasValue && request.PropertyUnitId.Value != relation.PropertyUnitId)
        {
            var newUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId.Value, cancellationToken)
                ?? throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");

            if (newUnit.BuildingId != survey.BuildingId)
                throw new ValidationException($"Property unit {request.PropertyUnitId} does not belong to survey building {survey.BuildingId}");

            relation.UpdatePropertyUnitId(request.PropertyUnitId.Value, currentUserId);
        }

        // Determine effective values for business validation
        var effectiveRelationType = request.RelationType.HasValue ? (RelationType)request.RelationType.Value : relation.RelationType;
        var effectiveOccupancyType = request.ClearOccupancyType ? null : (request.OccupancyType.HasValue ? (OccupancyType?)request.OccupancyType.Value : relation.OccupancyType);
        var effectiveHasEvidence = request.HasEvidence ?? relation.HasEvidence;
        var effectiveOwnershipShare = request.ClearOwnershipShare ? null : (request.OwnershipShare ?? relation.OwnershipShare);
        var effectiveContractDetails = request.ClearContractDetails ? null : (request.ContractDetails ?? relation.ContractDetails);
        var effectiveNotes = request.ClearNotes ? null : (request.Notes ?? relation.Notes);

        // Validate ownership share for Owner type
        if (effectiveRelationType == RelationType.Owner)
        {
            if (!effectiveOwnershipShare.HasValue || effectiveOwnershipShare <= 0)
                throw new ValidationException("Ownership share is required for Owner type and must be > 0");
            if (effectiveOwnershipShare > 1)
                throw new ValidationException("Ownership share cannot exceed 1.0 (100%)");
        }

        // Update using simplified domain method
        relation.UpdateRelationDetails(
            effectiveRelationType,
            effectiveOccupancyType,
            effectiveHasEvidence,
            effectiveOwnershipShare,
            effectiveContractDetails,
            effectiveNotes,
            currentUserId);

        await _relationRepository.UpdateAsync(relation, cancellationToken);
        await _relationRepository.SaveChangesAsync(cancellationToken);

        // Build changed fields list for audit
        var changedFields = new List<string>();
        if (request.PersonId.HasValue) changedFields.Add("PersonId");
        if (request.PropertyUnitId.HasValue) changedFields.Add("PropertyUnitId");
        if (request.RelationType.HasValue) changedFields.Add("RelationType");
        if (request.OccupancyType.HasValue || request.ClearOccupancyType) changedFields.Add("OccupancyType");
        if (request.HasEvidence.HasValue) changedFields.Add("HasEvidence");
        if (request.OwnershipShare.HasValue || request.ClearOwnershipShare) changedFields.Add("OwnershipShare");
        if (request.ContractDetails != null || request.ClearContractDetails) changedFields.Add("ContractDetails");
        if (request.Notes != null || request.ClearNotes) changedFields.Add("Notes");

        // Audit log
        await _auditService.LogActionAsync(
            AuditActionType.Update,
            $"Updated relation ({relation.RelationType}) for property unit {propertyUnit.UnitIdentifier}",
            "PersonPropertyRelation",
            relation.Id,
            relation.Id.ToString(),
            System.Text.Json.JsonSerializer.Serialize(oldValues),
            System.Text.Json.JsonSerializer.Serialize(new
            {
                RelationType = relation.RelationType.ToString(),
                ContractType = relation.ContractType?.ToString(),
                relation.OwnershipShare
            }),
            string.Join(", ", changedFields),
            cancellationToken);

        return MapToDto(relation);
    }

    private static PersonPropertyRelationDto MapToDto(PersonPropertyRelation r)
    {
        return new PersonPropertyRelationDto
        {
            Id = r.Id,
            PersonId = r.PersonId,
            PropertyUnitId = r.PropertyUnitId,
            RelationType = (int)r.RelationType,
            OccupancyType = r.OccupancyType.HasValue ? (int)r.OccupancyType.Value : (int?)null,
            HasEvidence = r.HasEvidence,
            OwnershipShare = r.OwnershipShare,
            ContractDetails = r.ContractDetails,
            Notes = r.Notes,
            IsActive = r.IsActive,
            CreatedAtUtc = r.CreatedAtUtc,
            CreatedBy = r.CreatedBy,
            LastModifiedAtUtc = r.LastModifiedAtUtc,
            LastModifiedBy = r.LastModifiedBy,
            IsDeleted = r.IsDeleted,
            DeletedAtUtc = r.DeletedAtUtc,
            DeletedBy = r.DeletedBy,
            IsOngoing = r.IsActive,
            EvidenceCount = r.EvidenceRelations?.Count(er => er.IsActive && !er.IsDeleted) ?? 0
        };
    }
}
