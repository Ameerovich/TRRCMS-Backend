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
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdatePersonPropertyRelationCommandHandler(
        ISurveyRepository surveyRepository,
        IPersonPropertyRelationRepository relationRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _surveyRepository = surveyRepository;
        _relationRepository = relationRepository;
        _propertyUnitRepository = propertyUnitRepository;
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
            RelationType = relation.RelationType.ToString(),
            relation.RelationTypeOtherDesc,
            ContractType = relation.ContractType?.ToString(),
            relation.ContractTypeOtherDesc,
            relation.OwnershipShare,
            relation.ContractDetails,
            relation.StartDate,
            relation.EndDate,
            relation.Notes
        };

        // Determine effective values for business validation
        var effectiveRelationType = request.RelationType ?? relation.RelationType;

        // Validate ownership share for Owner type
        if (effectiveRelationType == RelationType.Owner)
        {
            var effectiveShare = request.ClearOwnershipShare ? null : (request.OwnershipShare ?? relation.OwnershipShare);
            if (!effectiveShare.HasValue || effectiveShare <= 0)
                throw new ValidationException("Ownership share is required for Owner type and must be > 0");
            if (effectiveShare > 1)
                throw new ValidationException("Ownership share cannot exceed 1.0 (100%)");
        }

        // Validate Other description for RelationType.Other
        if (effectiveRelationType == RelationType.Other)
        {
            var effectiveDesc = request.ClearRelationTypeOtherDesc ? null : (request.RelationTypeOtherDesc ?? relation.RelationTypeOtherDesc);
            if (string.IsNullOrWhiteSpace(effectiveDesc))
                throw new ValidationException("Description required when relation type is 'Other'");
        }

        // Validate Other description for ContractType.Other
        var effectiveContractType = request.ClearContractType ? null : (request.ContractType ?? relation.ContractType);
        if (effectiveContractType == TenureContractType.Other)
        {
            var effectiveDesc = request.ClearContractTypeOtherDesc ? null : (request.ContractTypeOtherDesc ?? relation.ContractTypeOtherDesc);
            if (string.IsNullOrWhiteSpace(effectiveDesc))
                throw new ValidationException("Description required when contract type is 'Other'");
        }

        // Validate date range
        var effectiveStartDate = request.ClearStartDate ? null : (request.StartDate ?? relation.StartDate);
        var effectiveEndDate = request.ClearEndDate ? null : (request.EndDate ?? relation.EndDate);
        if (effectiveStartDate.HasValue && effectiveEndDate.HasValue && effectiveEndDate < effectiveStartDate)
            throw new ValidationException("End date cannot be before start date");

        // Perform partial update using domain method
        relation.PartialUpdate(
            request.RelationType,
            request.RelationTypeOtherDesc,
            request.ContractType,
            request.ContractTypeOtherDesc,
            request.OwnershipShare,
            request.ContractDetails,
            request.StartDate,
            request.EndDate,
            request.Notes,
            request.ClearRelationTypeOtherDesc,
            request.ClearContractType,
            request.ClearContractTypeOtherDesc,
            request.ClearOwnershipShare,
            request.ClearContractDetails,
            request.ClearStartDate,
            request.ClearEndDate,
            request.ClearNotes,
            currentUserId);

        await _relationRepository.UpdateAsync(relation, cancellationToken);
        await _relationRepository.SaveChangesAsync(cancellationToken);

        // Build changed fields list for audit
        var changedFields = new List<string>();
        if (request.RelationType.HasValue) changedFields.Add("RelationType");
        if (request.RelationTypeOtherDesc != null || request.ClearRelationTypeOtherDesc) changedFields.Add("RelationTypeOtherDesc");
        if (request.ContractType.HasValue || request.ClearContractType) changedFields.Add("ContractType");
        if (request.ContractTypeOtherDesc != null || request.ClearContractTypeOtherDesc) changedFields.Add("ContractTypeOtherDesc");
        if (request.OwnershipShare.HasValue || request.ClearOwnershipShare) changedFields.Add("OwnershipShare");
        if (request.ContractDetails != null || request.ClearContractDetails) changedFields.Add("ContractDetails");
        if (request.StartDate.HasValue || request.ClearStartDate) changedFields.Add("StartDate");
        if (request.EndDate.HasValue || request.ClearEndDate) changedFields.Add("EndDate");
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
        int? duration = null;
        bool ongoing = false;
        if (r.StartDate.HasValue)
        {
            if (r.EndDate.HasValue)
                duration = (int)(r.EndDate.Value - r.StartDate.Value).TotalDays;
            else
            {
                ongoing = true;
                duration = (int)(DateTime.UtcNow - r.StartDate.Value).TotalDays;
            }
        }

        return new PersonPropertyRelationDto
        {
            Id = r.Id,
            PersonId = r.PersonId,
            PropertyUnitId = r.PropertyUnitId,
            RelationType = r.RelationType,
            RelationTypeOtherDesc = r.RelationTypeOtherDesc,
            ContractType = r.ContractType,
            ContractTypeOtherDesc = r.ContractTypeOtherDesc,
            OwnershipShare = r.OwnershipShare,
            ContractDetails = r.ContractDetails,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            Notes = r.Notes,
            IsActive = r.IsActive,
            CreatedAtUtc = r.CreatedAtUtc,
            CreatedBy = r.CreatedBy,
            LastModifiedAtUtc = r.LastModifiedAtUtc,
            LastModifiedBy = r.LastModifiedBy,
            IsDeleted = r.IsDeleted,
            DeletedAtUtc = r.DeletedAtUtc,
            DeletedBy = r.DeletedBy,
            DurationInDays = duration,
            IsOngoing = ongoing,
            EvidenceCount = r.Evidences?.Count ?? 0
        };
    }
}
