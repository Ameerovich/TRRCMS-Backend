using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.DeleteBuilding;

/// <summary>
/// Handler for DeleteBuildingCommand
/// Performs cascade soft delete of building and all related entities
/// </summary>
public class DeleteBuildingCommandHandler : IRequestHandler<DeleteBuildingCommand, DeleteResultDto>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeleteBuildingCommandHandler(
        IBuildingRepository buildingRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IHouseholdRepository householdRepository,
        IPersonRepository personRepository,
        IPersonPropertyRelationRepository relationRepository,
        IEvidenceRepository evidenceRepository,
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _buildingRepository = buildingRepository;
        _propertyUnitRepository = propertyUnitRepository;
        _householdRepository = householdRepository;
        _personRepository = personRepository;
        _relationRepository = relationRepository;
        _evidenceRepository = evidenceRepository;
        _surveyRepository = surveyRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<DeleteResultDto> Handle(DeleteBuildingCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var affectedEntities = new List<DeletedEntityInfo>();

        // Get building
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId, cancellationToken)
            ?? throw new NotFoundException($"Building with ID {request.BuildingId} not found");

        if (building.IsDeleted)
            throw new ValidationException("Building is already deleted");

        // Validate survey status
        var surveys = await _surveyRepository.GetByBuildingAsync(request.BuildingId, cancellationToken);
        var activeSurvey = surveys.FirstOrDefault(s => !s.IsDeleted);

        if (activeSurvey != null && activeSurvey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot delete building. Survey status is {activeSurvey.Status}. Only surveys in Draft status can be modified.");

        // CASCADE DELETE all property units and their descendants
        var propertyUnits = await _propertyUnitRepository.GetByBuildingIdAsync(request.BuildingId, cancellationToken);

        foreach (var propertyUnit in propertyUnits.Where(pu => !pu.IsDeleted))
        {
            await DeletePropertyUnitCascade(propertyUnit.Id, currentUserId, affectedEntities, cancellationToken);
        }

        // Soft delete building
        building.MarkAsDeleted(currentUserId);
        await _buildingRepository.UpdateAsync(building, cancellationToken);

        affectedEntities.Insert(0, new DeletedEntityInfo
        {
            EntityId = building.Id,
            EntityType = "Building",
            EntityIdentifier = building.BuildingId
        });

        await _buildingRepository.SaveChangesAsync(cancellationToken);

        var stats = CalculateStatistics(affectedEntities);

        await _auditService.LogActionAsync(
            AuditActionType.Delete,
            $"Deleted Building {building.BuildingId} with cascades",
            "Building",
            building.Id,
            building.BuildingId,
            null,
            null,
            $"Cascaded: {stats.PropertyUnits} units, {stats.Households} households, {stats.Persons} persons, {stats.Relations} relations, {stats.Evidences} evidences",
            cancellationToken);

        return new DeleteResultDto
        {
            PrimaryEntityId = building.Id,
            PrimaryEntityType = "Building",
            AffectedEntities = affectedEntities,
            TotalAffected = affectedEntities.Count,
            DeletedAtUtc = building.DeletedAtUtc!.Value,
            Message = $"Building deleted successfully along with {stats.PropertyUnits} property unit(s), {stats.Households} household(s), {stats.Persons} person(s), {stats.Relations} relation(s), and {stats.Evidences} evidence(s)"
        };
    }

    private async Task DeletePropertyUnitCascade(Guid propertyUnitId, Guid userId, List<DeletedEntityInfo> affected, CancellationToken ct)
    {
        var unit = await _propertyUnitRepository.GetByIdAsync(propertyUnitId, ct);
        if (unit == null || unit.IsDeleted) return;

        var households = await _householdRepository.GetByPropertyUnitIdAsync(propertyUnitId, ct);
        foreach (var household in households.Where(h => !h.IsDeleted))
        {
            var persons = await _personRepository.GetByHouseholdIdAsync(household.Id, ct);
            foreach (var person in persons.Where(p => !p.IsDeleted))
            {
                await DeletePersonCascade(person.Id, userId, affected, ct);
            }
            household.MarkAsDeleted(userId);
            await _householdRepository.UpdateAsync(household, ct);
            affected.Add(new DeletedEntityInfo { EntityId = household.Id, EntityType = "Household", EntityIdentifier = household.HeadOfHouseholdName ?? "Household" });
        }

        var relations = await _relationRepository.GetByPropertyUnitIdAsync(propertyUnitId, ct);
        foreach (var relation in relations.Where(r => !r.IsDeleted && !affected.Any(e => e.EntityId == r.Id)))
        {
            await DeleteRelationCascade(relation.Id, userId, affected, ct);
        }

        unit.MarkAsDeleted(userId);
        await _propertyUnitRepository.UpdateAsync(unit, ct);
        affected.Add(new DeletedEntityInfo { EntityId = unit.Id, EntityType = "PropertyUnit", EntityIdentifier = unit.UnitIdentifier });
    }

    private async Task DeletePersonCascade(Guid personId, Guid userId, List<DeletedEntityInfo> affected, CancellationToken ct)
    {
        var person = await _personRepository.GetByIdAsync(personId, ct);
        if (person == null || person.IsDeleted) return;

        var relations = await _relationRepository.GetByPersonIdAsync(personId, ct);
        foreach (var relation in relations.Where(r => !r.IsDeleted))
        {
            await DeleteRelationCascade(relation.Id, userId, affected, ct);
        }

        var evidences = await _evidenceRepository.GetByPersonIdAsync(personId, ct);
        foreach (var evidence in evidences.Where(e => !e.IsDeleted && !e.EvidenceRelations.Any(er => er.IsActive && !er.IsDeleted) && !affected.Any(ae => ae.EntityId == e.Id)))
        {
            evidence.MarkAsDeleted(userId);
            await _evidenceRepository.UpdateAsync(evidence, ct);
            affected.Add(new DeletedEntityInfo { EntityId = evidence.Id, EntityType = "Evidence", EntityIdentifier = evidence.OriginalFileName });
        }

        person.MarkAsDeleted(userId);
        await _personRepository.UpdateAsync(person, ct);
        affected.Add(new DeletedEntityInfo { EntityId = person.Id, EntityType = "Person", EntityIdentifier = person.GetFullNameArabic() });
    }

    private async Task DeleteRelationCascade(Guid relationId, Guid userId, List<DeletedEntityInfo> affected, CancellationToken ct)
    {
        var relation = await _relationRepository.GetByIdAsync(relationId, ct);
        if (relation == null || relation.IsDeleted) return;

        var evidences = await _evidenceRepository.GetByRelationIdAsync(relationId, ct);
        foreach (var evidence in evidences.Where(e => !e.IsDeleted && !affected.Any(ae => ae.EntityId == e.Id)))
        {
            evidence.MarkAsDeleted(userId);
            await _evidenceRepository.UpdateAsync(evidence, ct);
            affected.Add(new DeletedEntityInfo { EntityId = evidence.Id, EntityType = "Evidence", EntityIdentifier = evidence.OriginalFileName });
        }

        relation.MarkAsDeleted(userId);
        await _relationRepository.UpdateAsync(relation, ct);
        affected.Add(new DeletedEntityInfo { EntityId = relation.Id, EntityType = "PersonPropertyRelation", EntityIdentifier = $"Relation {relation.RelationType}" });
    }

    private (int PropertyUnits, int Households, int Persons, int Relations, int Evidences) CalculateStatistics(List<DeletedEntityInfo> entities)
    {
        return (
            entities.Count(e => e.EntityType == "PropertyUnit"),
            entities.Count(e => e.EntityType == "Household"),
            entities.Count(e => e.EntityType == "Person"),
            entities.Count(e => e.EntityType == "PersonPropertyRelation"),
            entities.Count(e => e.EntityType == "Evidence")
        );
    }
}
