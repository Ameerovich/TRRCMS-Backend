using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;

public class LinkPersonToPropertyUnitCommandHandler : IRequestHandler<LinkPersonToPropertyUnitCommand, PersonPropertyRelationDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public LinkPersonToPropertyUnitCommandHandler(
        ISurveyRepository surveyRepository,
        IPersonRepository personRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IPersonPropertyRelationRepository relationRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _surveyRepository = surveyRepository;
        _personRepository = personRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _relationRepository = relationRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<PersonPropertyRelationDto> Handle(LinkPersonToPropertyUnitCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get and validate survey
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only link persons in your own surveys");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot modify survey in {survey.Status} status");

        // Get and validate person
        var person = await _personRepository.GetByIdAsync(request.PersonId, cancellationToken)
            ?? throw new NotFoundException($"Person with ID {request.PersonId} not found");

        // Get and validate property unit
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId, cancellationToken)
            ?? throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");

        if (propertyUnit.BuildingId != survey.BuildingId)
            throw new ValidationException("Property unit does not belong to the survey's building");

        // Check for existing relation
        var existingRelation = await _relationRepository.GetByPersonAndPropertyUnitAsync(
            request.PersonId, request.PropertyUnitId, cancellationToken);
        if (existingRelation != null)
            throw new ValidationException($"Person is already linked with relation type: {existingRelation.RelationType}");

        // Business validations
        if ((RelationType)request.RelationType == RelationType.Owner)
        {
            if (!request.OwnershipShare.HasValue || request.OwnershipShare <= 0)
                throw new ValidationException("Ownership share is required for Owner type and must be > 0");
            if (request.OwnershipShare > 1)
                throw new ValidationException("Ownership share cannot exceed 1.0 (100%)");
        }

        // Create relation using factory method — link to originating survey
        var relation = PersonPropertyRelation.Create(
            request.PersonId,
            request.PropertyUnitId,
            (RelationType)request.RelationType,
            request.OccupancyType.HasValue ? (OccupancyType)request.OccupancyType.Value : (OccupancyType?)null,
            request.HasEvidence,
            currentUserId,
            surveyId: request.SurveyId);

        // Update with additional details using simplified signature
        relation.UpdateRelationDetails(
            (RelationType)request.RelationType,
            request.OccupancyType.HasValue ? (OccupancyType)request.OccupancyType.Value : (OccupancyType?)null,
            request.HasEvidence,
            request.OwnershipShare,
            request.ContractDetails,
            request.Notes,
            currentUserId);

        await _relationRepository.AddAsync(relation, cancellationToken);
        await _relationRepository.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogActionAsync(
            AuditActionType.Create,
            $"Linked person {person.GetFullNameArabic()} to property unit {propertyUnit.UnitIdentifier} as {(RelationType)request.RelationType}",
            "PersonPropertyRelation",
            relation.Id,
            $"{person.GetFullNameArabic()} - {propertyUnit.UnitIdentifier}",
            null,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                relation.Id,
                request.PersonId,
                request.PropertyUnitId,
                RelationType = ((RelationType)request.RelationType).ToString(),
                OccupancyType = request.OccupancyType.HasValue ? ((OccupancyType)request.OccupancyType.Value).ToString() : null,
                request.HasEvidence,
                request.OwnershipShare
            }),
            "New Person-Property Relation",
            cancellationToken);

        return MapToDto(relation);
    }

    /// <summary>
    /// Maps PersonPropertyRelation entity to PersonPropertyRelationDto.
    /// Updated for office survey workflow - removed deprecated fields.
    /// </summary>
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
            EvidenceCount = r.Evidences?.Count ?? 0
        };
    }
}
