using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.DeleteHouseholdInSurvey;

/// <summary>
/// Handler for DeleteHouseholdInSurveyCommand.
/// Cascade soft-deletes household → persons → relations → evidences.
/// حذف الأسرة مع جميع البيانات المرتبطة
/// </summary>
public class DeleteHouseholdInSurveyCommandHandler : IRequestHandler<DeleteHouseholdInSurveyCommand, DeleteResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeleteHouseholdInSurveyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<DeleteResultDto> Handle(DeleteHouseholdInSurveyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate survey
        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_EditAll))
                throw new UnauthorizedAccessException("You can only delete households in your own surveys");
        }

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot delete household. Survey status is {survey.Status}. Only Draft surveys can be modified.");

        // Validate household
        var household = await _unitOfWork.Households.GetByIdAsync(request.HouseholdId, cancellationToken)
            ?? throw new NotFoundException($"Household with ID {request.HouseholdId} not found");

        if (household.IsDeleted)
            throw new ValidationException("Household is already deleted");

        // Verify household belongs to a property unit in survey's building
        var propertyUnit = await _unitOfWork.PropertyUnits.GetByIdAsync(household.PropertyUnitId, cancellationToken);
        if (propertyUnit == null || propertyUnit.BuildingId != survey.BuildingId)
            throw new ValidationException("Household does not belong to this survey's building");

        var affectedEntities = new List<DeletedEntityInfo>();

        // CASCADE: Delete persons in this household → their relations → their evidences
        var persons = await _unitOfWork.Persons.GetByHouseholdIdAsync(request.HouseholdId, cancellationToken);
        foreach (var person in persons.Where(p => !p.IsDeleted))
        {
            // Delete person's relations
            var relations = await _unitOfWork.PersonPropertyRelations.GetByPersonIdAsync(person.Id, cancellationToken);
            foreach (var relation in relations.Where(r => !r.IsDeleted))
            {
                // Delete evidences linked to relation
                var evidences = await _unitOfWork.Evidences.GetByRelationIdAsync(relation.Id, cancellationToken);
                foreach (var evidence in evidences.Where(e => !e.IsDeleted && !affectedEntities.Any(ae => ae.EntityId == e.Id)))
                {
                    evidence.MarkAsDeleted(currentUserId);
                    await _unitOfWork.Evidences.UpdateAsync(evidence, cancellationToken);
                    affectedEntities.Add(new DeletedEntityInfo
                    {
                        EntityId = evidence.Id,
                        EntityType = "Evidence",
                        EntityIdentifier = evidence.OriginalFileName
                    });
                }

                relation.MarkAsDeleted(currentUserId);
                await _unitOfWork.PersonPropertyRelations.UpdateAsync(relation, cancellationToken);
                affectedEntities.Add(new DeletedEntityInfo
                {
                    EntityId = relation.Id,
                    EntityType = "PersonPropertyRelation",
                    EntityIdentifier = $"Relation {relation.RelationType}"
                });
            }

            // Delete evidences linked directly to person (not via relation)
            var personEvidences = await _unitOfWork.Evidences.GetByPersonIdAsync(person.Id, cancellationToken);
            foreach (var evidence in personEvidences.Where(e => !e.IsDeleted && !affectedEntities.Any(ae => ae.EntityId == e.Id)))
            {
                evidence.MarkAsDeleted(currentUserId);
                await _unitOfWork.Evidences.UpdateAsync(evidence, cancellationToken);
                affectedEntities.Add(new DeletedEntityInfo
                {
                    EntityId = evidence.Id,
                    EntityType = "Evidence",
                    EntityIdentifier = evidence.OriginalFileName
                });
            }

            person.MarkAsDeleted(currentUserId);
            await _unitOfWork.Persons.UpdateAsync(person, cancellationToken);
            affectedEntities.Add(new DeletedEntityInfo
            {
                EntityId = person.Id,
                EntityType = "Person",
                EntityIdentifier = person.GetFullNameArabic()
            });
        }

        // Soft delete the household itself
        household.MarkAsDeleted(currentUserId);
        await _unitOfWork.Households.UpdateAsync(household, cancellationToken);
        affectedEntities.Insert(0, new DeletedEntityInfo
        {
            EntityId = household.Id,
            EntityType = "Household",
            EntityIdentifier = $"Household {household.Id.ToString()[..8]}"
        });

        // Save all changes atomically
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Counts for message
        int personCount = affectedEntities.Count(e => e.EntityType == "Person");
        int relationCount = affectedEntities.Count(e => e.EntityType == "PersonPropertyRelation");
        int evidenceCount = affectedEntities.Count(e => e.EntityType == "Evidence");

        // Audit
        var householdIdentifier = $"Household {household.Id.ToString()[..8]}";
        await _auditService.LogActionAsync(
            AuditActionType.Delete,
            $"Deleted {householdIdentifier} in survey {survey.ReferenceCode} with cascades",
            "Household",
            household.Id,
            householdIdentifier,
            null,
            null,
            $"Cascaded: {personCount} person(s), {relationCount} relation(s), {evidenceCount} evidence(s)",
            cancellationToken);

        return new DeleteResultDto
        {
            PrimaryEntityId = household.Id,
            PrimaryEntityType = "Household",
            AffectedEntities = affectedEntities,
            TotalAffected = affectedEntities.Count,
            DeletedAtUtc = household.DeletedAtUtc!.Value,
            Message = $"Household deleted successfully along with {personCount} person(s), {relationCount} relation(s), and {evidenceCount} evidence(s)"
        };
    }
}
