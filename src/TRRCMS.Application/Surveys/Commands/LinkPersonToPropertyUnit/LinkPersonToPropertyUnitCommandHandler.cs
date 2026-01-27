using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;

/// <summary>
/// Handler for LinkPersonToPropertyUnitCommand
/// Creates PersonPropertyRelation to track ownership/tenancy
/// Returns the created relation as DTO
/// </summary>
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
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _relationRepository = relationRepository ?? throw new ArgumentNullException(nameof(relationRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<PersonPropertyRelationDto> Handle(LinkPersonToPropertyUnitCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("You can only link persons to property units in your own surveys");
        }

        // Verify survey is in Draft status
        if (survey.Status != SurveyStatus.Draft)
        {
            throw new ValidationException($"Cannot link persons for survey in {survey.Status} status. Only Draft surveys can be modified.");
        }

        // Get and validate person
        var person = await _personRepository.GetByIdAsync(request.PersonId, cancellationToken);
        if (person == null)
        {
            throw new NotFoundException($"Person with ID {request.PersonId} not found");
        }

        // Get and validate property unit
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId, cancellationToken);
        if (propertyUnit == null)
        {
            throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");
        }

        // Verify property unit belongs to survey's building
        if (propertyUnit.BuildingId != survey.BuildingId)
        {
            throw new ValidationException("Property unit does not belong to the survey's building");
        }

        // Check if relation already exists
        var existingRelation = await _relationRepository.GetByPersonAndPropertyUnitAsync(
            request.PersonId,
            request.PropertyUnitId,
            cancellationToken);

        if (existingRelation != null)
        {
            throw new ValidationException($"Person is already linked to this property unit with relation type: {existingRelation.RelationType}");
        }

        // Validate ownership share for Owner relation type
        if (request.RelationType.Equals("Owner", StringComparison.OrdinalIgnoreCase))
        {
            if (!request.OwnershipShare.HasValue || request.OwnershipShare <= 0)
            {
                throw new ValidationException("Ownership share is required for Owner relation type and must be greater than 0");
            }
            if (request.OwnershipShare > 1)
            {
                throw new ValidationException("Ownership share cannot exceed 1.0 (100%)");
            }
        }

        // Validate RelationTypeOtherDesc when RelationType is "Other"
        if (request.RelationType.Equals("Other", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(request.RelationTypeOtherDesc))
        {
            throw new ValidationException("Description is required when relation type is 'Other'");
        }

        // Validate date range
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
        {
            throw new ValidationException("End date cannot be before start date");
        }

        // Step 1: Create PersonPropertyRelation with required fields using factory method
        var relation = PersonPropertyRelation.Create(
            personId: request.PersonId,
            propertyUnitId: request.PropertyUnitId,
            relationType: request.RelationType,
            createdByUserId: currentUserId
        );

        // Step 2: Update optional fields using the domain method (respects private setters)
        // This is the correct way to set fields with private setters in DDD
        relation.UpdateRelationDetails(
            relationType: request.RelationType,
            relationTypeOtherDesc: request.RelationTypeOtherDesc,
            ownershipShare: request.OwnershipShare,
            contractDetails: request.ContractDetails,
            startDate: request.StartDate,
            endDate: request.EndDate,
            notes: request.Notes,
            modifiedByUserId: currentUserId
        );

        // Save relation
        await _relationRepository.AddAsync(relation, cancellationToken);
        await _relationRepository.SaveChangesAsync(cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Linked person {person.GetFullNameArabic()} to property unit {propertyUnit.UnitIdentifier} as {request.RelationType} in survey {survey.ReferenceCode}",
            entityType: "PersonPropertyRelation",
            entityId: relation.Id,
            entityIdentifier: $"{person.GetFullNameArabic()} - {propertyUnit.UnitIdentifier}",
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                relation.Id,
                request.PersonId,
                PersonName = person.GetFullNameArabic(),
                request.PropertyUnitId,
                PropertyUnitIdentifier = propertyUnit.UnitIdentifier,
                request.RelationType,
                request.RelationTypeOtherDesc,
                request.OwnershipShare,
                request.ContractDetails,
                request.StartDate,
                request.EndDate,
                request.Notes
            }),
            changedFields: "New Person-Property Relation",
            cancellationToken: cancellationToken
        );

        // Map to DTO and return
        return MapToDto(relation);
    }

    /// <summary>
    /// Maps PersonPropertyRelation entity to DTO
    /// </summary>
    private static PersonPropertyRelationDto MapToDto(PersonPropertyRelation relation)
    {
        // Calculate computed properties
        int? durationInDays = null;
        bool isOngoing = false;

        if (relation.StartDate.HasValue)
        {
            if (relation.EndDate.HasValue)
            {
                durationInDays = (int)(relation.EndDate.Value - relation.StartDate.Value).TotalDays;
                isOngoing = false;
            }
            else
            {
                // Relation is ongoing (has start but no end)
                isOngoing = true;
                durationInDays = (int)(DateTime.UtcNow - relation.StartDate.Value).TotalDays;
            }
        }

        return new PersonPropertyRelationDto
        {
            // Identifiers
            Id = relation.Id,
            PersonId = relation.PersonId,
            PropertyUnitId = relation.PropertyUnitId,

            // Relation attributes
            RelationType = relation.RelationType,
            RelationTypeOtherDesc = relation.RelationTypeOtherDesc,
            OwnershipShare = relation.OwnershipShare,
            ContractDetails = relation.ContractDetails,
            StartDate = relation.StartDate,
            EndDate = relation.EndDate,
            Notes = relation.Notes,
            IsActive = relation.IsActive,

            // Audit fields
            CreatedAtUtc = relation.CreatedAtUtc,
            CreatedBy = relation.CreatedBy,
            LastModifiedAtUtc = relation.LastModifiedAtUtc,
            LastModifiedBy = relation.LastModifiedBy,
            IsDeleted = relation.IsDeleted,
            DeletedAtUtc = relation.DeletedAtUtc,
            DeletedBy = relation.DeletedBy,

            // Computed properties
            DurationInDays = durationInDays,
            IsOngoing = isOngoing
        };
    }
}