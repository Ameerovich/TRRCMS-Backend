using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PropertyUnits.Commands.DeletePropertyUnit;

public class DeletePropertyUnitCommandHandler : IRequestHandler<DeletePropertyUnitCommand, DeleteResultDto>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeletePropertyUnitCommandHandler(
        IPropertyUnitRepository propertyUnitRepository,
        IHouseholdRepository householdRepository,
        IPersonRepository personRepository,
        IPersonPropertyRelationRepository relationRepository,
        IEvidenceRepository evidenceRepository,
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _propertyUnitRepository = propertyUnitRepository;
        _householdRepository = householdRepository;
        _personRepository = personRepository;
        _relationRepository = relationRepository;
        _evidenceRepository = evidenceRepository;
        _surveyRepository = surveyRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<DeleteResultDto> Handle(DeletePropertyUnitCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var affectedEntities = new List<DeletedEntityInfo>();

        // Get property unit
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId, cancellationToken)
            ?? throw new NotFoundException($"PropertyUnit with ID {request.PropertyUnitId} not found");

        if (propertyUnit.IsDeleted)
            throw new ValidationException("PropertyUnit is already deleted");

        // Validate survey status
        var surveys = await _surveyRepository.GetByBuildingAsync(propertyUnit.BuildingId, cancellationToken);
        var survey = surveys.FirstOrDefault(s => !s.IsDeleted);

        if (survey != null && survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot delete property unit. Survey status is {survey.Status}. Only surveys in Draft status can be modified.");

        // CASCADE DELETE 1: Get all households in this property unit
        var households = await _householdRepository.GetByPropertyUnitIdAsync(request.PropertyUnitId, cancellationToken);

        foreach (var household in households.Where(h => !h.IsDeleted))
        {
            // CASCADE DELETE 1a: Get all persons in this household
            var householdPersons = await _personRepository.GetByHouseholdIdAsync(household.Id, cancellationToken);

            foreach (var person in householdPersons.Where(p => !p.IsDeleted))
            {
                // CASCADE DELETE 1a-i: Delete all person's relations and their evidences
                var personRelations = await _relationRepository.GetByPersonIdAsync(person.Id, cancellationToken);

                foreach (var relation in personRelations.Where(r => !r.IsDeleted))
                {
                    // Delete evidences linked to relation
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

                // CASCADE DELETE 1a-ii: Delete evidences directly linked to person
                var personEvidences = await _evidenceRepository.GetByPersonIdAsync(person.Id, cancellationToken);
                foreach (var evidence in personEvidences.Where(e => !e.IsDeleted && !e.PersonPropertyRelationId.HasValue))
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

                // Delete the person
                person.MarkAsDeleted(currentUserId);
                await _personRepository.UpdateAsync(person, cancellationToken);

                affectedEntities.Add(new DeletedEntityInfo
                {
                    EntityId = person.Id,
                    EntityType = "Person",
                    EntityIdentifier = person.GetFullNameArabic()
                });
            }

            // Delete the household
            household.MarkAsDeleted(currentUserId);
            await _householdRepository.UpdateAsync(household, cancellationToken);

            affectedEntities.Add(new DeletedEntityInfo
            {
                EntityId = household.Id,
                EntityType = "Household",
                EntityIdentifier = household.HeadOfHouseholdName ?? "Household"
            });
        }

        // CASCADE DELETE 2: Delete relations directly linked to this property unit (not through households)
        var unitRelations = await _relationRepository.GetByPropertyUnitIdAsync(request.PropertyUnitId, cancellationToken);
        foreach (var relation in unitRelations.Where(r => !r.IsDeleted))
        {
            var relationEvidences = await _evidenceRepository.GetByRelationIdAsync(relation.Id, cancellationToken);
            foreach (var evidence in relationEvidences.Where(e => !e.IsDeleted))
            {
                evidence.MarkAsDeleted(currentUserId);
                await _evidenceRepository.UpdateAsync(evidence, cancellationToken);

                if (!affectedEntities.Any(e => e.EntityId == evidence.Id))
                {
                    affectedEntities.Add(new DeletedEntityInfo
                    {
                        EntityId = evidence.Id,
                        EntityType = "Evidence",
                        EntityIdentifier = evidence.OriginalFileName
                    });
                }
            }

            if (!affectedEntities.Any(e => e.EntityId == relation.Id))
            {
                relation.MarkAsDeleted(currentUserId);
                await _relationRepository.UpdateAsync(relation, cancellationToken);

                affectedEntities.Add(new DeletedEntityInfo
                {
                    EntityId = relation.Id,
                    EntityType = "PersonPropertyRelation",
                    EntityIdentifier = $"Relation {relation.RelationType}"
                });
            }
        }

        // Soft delete property unit
        propertyUnit.MarkAsDeleted(currentUserId);
        await _propertyUnitRepository.UpdateAsync(propertyUnit, cancellationToken);

        affectedEntities.Insert(0, new DeletedEntityInfo
        {
            EntityId = propertyUnit.Id,
            EntityType = "PropertyUnit",
            EntityIdentifier = propertyUnit.UnitIdentifier
        });

        await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

        var householdsCount = affectedEntities.Count(e => e.EntityType == "Household");
        var personsCount = affectedEntities.Count(e => e.EntityType == "Person");
        var relationsCount = affectedEntities.Count(e => e.EntityType == "PersonPropertyRelation");
        var evidencesCount = affectedEntities.Count(e => e.EntityType == "Evidence");

        // Audit log
        await _auditService.LogActionAsync(
            AuditActionType.Delete,
            $"Deleted PropertyUnit {propertyUnit.UnitIdentifier} with cascades",
            "PropertyUnit",
            propertyUnit.Id,
            propertyUnit.UnitIdentifier,
            null,
            null,
            $"Cascaded to {householdsCount} households, {personsCount} persons, {relationsCount} relations, {evidencesCount} evidences",
            cancellationToken);

        return new DeleteResultDto
        {
            PrimaryEntityId = propertyUnit.Id,
            PrimaryEntityType = "PropertyUnit",
            AffectedEntities = affectedEntities,
            TotalAffected = affectedEntities.Count,
            DeletedAtUtc = propertyUnit.DeletedAtUtc!.Value,
            Message = $"PropertyUnit deleted successfully along with {householdsCount} household(s), " +
                     $"{personsCount} person(s), {relationsCount} relation(s), and {evidencesCount} evidence(s)"
        };
    }
}
