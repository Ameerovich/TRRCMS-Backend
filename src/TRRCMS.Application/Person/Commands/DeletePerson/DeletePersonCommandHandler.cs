using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Persons.Commands.DeletePerson;

public class DeletePersonCommandHandler : IRequestHandler<DeletePersonCommand, DeleteResultDto>
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeletePersonCommandHandler(
        IPersonRepository personRepository,
        IPersonPropertyRelationRepository relationRepository,
        IEvidenceRepository evidenceRepository,
        IHouseholdRepository householdRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _personRepository = personRepository;
        _relationRepository = relationRepository;
        _evidenceRepository = evidenceRepository;
        _householdRepository = householdRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _surveyRepository = surveyRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<DeleteResultDto> Handle(DeletePersonCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var affectedEntities = new List<DeletedEntityInfo>();

        // Get person
        var person = await _personRepository.GetByIdAsync(request.PersonId, cancellationToken)
            ?? throw new NotFoundException($"Person with ID {request.PersonId} not found");

        if (person.IsDeleted)
            throw new ValidationException("Person is already deleted");

        // Validate survey status if person belongs to a household
        if (person.HouseholdId.HasValue)
        {
            var household = await _householdRepository.GetByIdAsync(person.HouseholdId.Value, cancellationToken);
            if (household != null)
            {
                var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);
                if (propertyUnit != null)
                {
                    var surveys = await _surveyRepository.GetByBuildingAsync(propertyUnit.BuildingId, cancellationToken);
                    var survey = surveys.FirstOrDefault(s => !s.IsDeleted);

                    if (survey != null && survey.Status != SurveyStatus.Draft)
                        throw new ValidationException($"Cannot delete person. Survey status is {survey.Status}. Only surveys in Draft status can be modified.");
                }
            }
        }

        // CASCADE DELETE 1: Delete all PersonPropertyRelations for this person
        var relations = await _relationRepository.GetByPersonIdAsync(request.PersonId, cancellationToken);
        foreach (var relation in relations.Where(r => !r.IsDeleted))
        {
            // CASCADE DELETE 1a: Delete evidences linked to this relation
            var relationEvidences = await _evidenceRepository.GetByRelationIdAsync(relation.Id, cancellationToken);
            foreach (var evidence in relationEvidences.Where(e => !e.IsDeleted))
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

            // Delete the relation
            relation.MarkAsDeleted(currentUserId);
            await _relationRepository.UpdateAsync(relation, cancellationToken);

            affectedEntities.Add(new DeletedEntityInfo
            {
                EntityId = relation.Id,
                EntityType = "PersonPropertyRelation",
                EntityIdentifier = $"Relation {relation.RelationType}"
            });
        }

        // CASCADE DELETE 2: Delete evidences directly linked to this person (not through relations)
        var personEvidences = await _evidenceRepository.GetByPersonIdAsync(request.PersonId, cancellationToken);
        foreach (var evidence in personEvidences.Where(e => !e.IsDeleted && !e.EvidenceRelations.Any(er => er.IsActive && !er.IsDeleted)))
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

        // Soft delete person
        person.MarkAsDeleted(currentUserId);
        await _personRepository.UpdateAsync(person, cancellationToken);

        affectedEntities.Insert(0, new DeletedEntityInfo
        {
            EntityId = person.Id,
            EntityType = "Person",
            EntityIdentifier = person.GetFullNameArabic()
        });

        await _personRepository.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogActionAsync(
            AuditActionType.Delete,
            $"Deleted person {person.GetFullNameArabic()} and {relations.Count()} relations with their evidences",
            "Person",
            person.Id,
            person.GetFullNameArabic(),
            null,
            null,
            $"Cascaded to {relations.Count()} relations and {affectedEntities.Count(e => e.EntityType == "Evidence")} evidences",
            cancellationToken);

        return new DeleteResultDto
        {
            PrimaryEntityId = person.Id,
            PrimaryEntityType = "Person",
            AffectedEntities = affectedEntities,
            TotalAffected = affectedEntities.Count,
            DeletedAtUtc = person.DeletedAtUtc!.Value,
            Message = $"Person deleted successfully along with {relations.Count()} relation(s) and their evidence(s)"
        };
    }
}
