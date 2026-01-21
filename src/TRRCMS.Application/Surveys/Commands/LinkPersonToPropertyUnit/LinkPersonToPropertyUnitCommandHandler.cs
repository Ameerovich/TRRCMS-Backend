using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;

/// <summary>
/// Handler for LinkPersonToPropertyUnitCommand
/// Creates PersonPropertyRelation to track ownership/tenancy
/// </summary>
public class LinkPersonToPropertyUnitCommandHandler : IRequestHandler<LinkPersonToPropertyUnitCommand, Unit>
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

    public async Task<Unit> Handle(LinkPersonToPropertyUnitCommand request, CancellationToken cancellationToken)
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

        // Create PersonPropertyRelation
        var relation = PersonPropertyRelation.Create(
            personId: request.PersonId,
            propertyUnitId: request.PropertyUnitId,
            relationType: request.RelationType,
            createdByUserId: currentUserId
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
                request.PersonId,
                PersonName = person.GetFullNameArabic(),
                request.PropertyUnitId,
                PropertyUnitIdentifier = propertyUnit.UnitIdentifier,
                request.RelationType,
            }),
            changedFields: "New Person-Property Relation",
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}