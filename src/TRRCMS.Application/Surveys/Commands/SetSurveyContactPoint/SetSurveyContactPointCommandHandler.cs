using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.SetSurveyContactPoint;

public class SetSurveyContactPointCommandHandler : IRequestHandler<SetSurveyContactPointCommand, PersonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IPersonDesignationService _designationService;
    private readonly IMapper _mapper;

    public SetSurveyContactPointCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IPersonDesignationService designationService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _designationService = designationService;
        _mapper = mapper;
    }

    public async Task<PersonDto> Handle(SetSurveyContactPointCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_EditAll))
                throw new UnauthorizedAccessException("You can only change the contact point of your own surveys");
        }

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException(
                $"Cannot change the contact point of a survey in {survey.Status} status. Only Draft surveys can be modified.");

        var person = await _unitOfWork.Persons.GetByIdAsync(request.PersonId, cancellationToken)
            ?? throw new NotFoundException($"Person with ID {request.PersonId} not found");

        if (!await BelongsToSurveyAsync(person, survey, cancellationToken))
            throw new ValidationException(
                "The person is not part of this survey. Add them to a household of the survey's property unit or link them to the unit first.");

        var previousContactId = survey.ContactPersonId;
        if (previousContactId != person.Id)
        {
            survey.SetContactPerson(person.Id, person.GetContactPersonFullName(), currentUserId);
            person.SetAsContactPerson(true, currentUserId);

            await ContactPointFlagSync.ReleasePreviousContactAsync(
                _unitOfWork, previousContactId, person.Id, survey.Id, currentUserId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditService.LogActionAsync(
                actionType: AuditActionType.Update,
                actionDescription: $"Set contact point of survey {survey.ReferenceCode} to {person.GetFullNameArabic()}",
                entityType: "Survey",
                entityId: survey.Id,
                entityIdentifier: survey.ReferenceCode,
                oldValues: System.Text.Json.JsonSerializer.Serialize(new { ContactPersonId = previousContactId }),
                newValues: System.Text.Json.JsonSerializer.Serialize(new { ContactPersonId = person.Id }),
                changedFields: "ContactPersonId, ContactPersonFullName",
                cancellationToken: cancellationToken);
        }

        var dto = _mapper.Map<PersonDto>(person);
        await _designationService.ApplyForSurveyAsync(new[] { dto }, survey, cancellationToken);
        return dto;
    }

    private async Task<bool> BelongsToSurveyAsync(
        Domain.Entities.Person person, Domain.Entities.Survey survey, CancellationToken cancellationToken)
    {
        if (survey.ContactPersonId == person.Id)
            return true;
        if (!survey.PropertyUnitId.HasValue)
            return false;

        // Member of a household on the survey's unit
        if (person.HouseholdId.HasValue)
        {
            var household = await _unitOfWork.Households.GetByIdAsync(person.HouseholdId.Value, cancellationToken);
            if (household?.PropertyUnitId == survey.PropertyUnitId)
                return true;
        }

        // Rights claimant on the survey's unit
        var withRelation = await _unitOfWork.PersonPropertyRelations.GetPersonIdsWithActiveRelationsAsync(
            new[] { person.Id }, survey.PropertyUnitId, cancellationToken);
        if (withRelation.Count > 0)
            return true;

        // Contact point of another survey on the same unit
        var unitSurveys = await _unitOfWork.Surveys.GetByPropertyUnitAsync(survey.PropertyUnitId.Value, cancellationToken);
        if (unitSurveys.Any(s => s.ContactPersonId == person.Id))
            return true;

        // Stand-alone contact person (created via POST contact-person, no household, no relations)
        // that was replaced earlier — allow re-designating them instead of creating a duplicate
        if (!person.HouseholdId.HasValue)
        {
            var anyRelation = await _unitOfWork.PersonPropertyRelations.GetPersonIdsWithActiveRelationsAsync(
                new[] { person.Id }, null, cancellationToken);
            return anyRelation.Count == 0;
        }

        return false;
    }
}
