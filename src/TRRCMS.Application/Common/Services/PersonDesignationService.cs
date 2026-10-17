using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Mappings;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Services;

/// <inheritdoc />
public class PersonDesignationService : IPersonDesignationService
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly ISurveyRepository _surveyRepository;

    public PersonDesignationService(
        IPersonRepository personRepository,
        IPersonPropertyRelationRepository relationRepository,
        ISurveyRepository surveyRepository)
    {
        _personRepository = personRepository;
        _relationRepository = relationRepository;
        _surveyRepository = surveyRepository;
    }

    public async Task ApplyAsync(IReadOnlyCollection<PersonDto> persons, CancellationToken cancellationToken = default)
    {
        if (persons.Count == 0) return;

        var ids = persons.Select(p => p.Id).ToList();
        var claimants = await _relationRepository.GetPersonIdsWithActiveRelationsAsync(ids, null, cancellationToken);
        var contacts = await _surveyRepository.GetContactPersonIdsAsync(ids, null, cancellationToken);

        foreach (var dto in persons)
        {
            // Keep the legacy flag (set on persons committed before the survey link existed)
            dto.IsContactPerson = dto.IsContactPerson || contacts.Contains(dto.Id);
            dto.IsRightsClaimant = claimants.Contains(dto.Id);
        }
    }

    public async Task ApplyForSurveyAsync(IReadOnlyCollection<PersonDto> persons, Survey survey, CancellationToken cancellationToken = default)
    {
        if (persons.Count == 0) return;

        // No unit linked yet: nobody can hold a claim on "the survey's unit"
        var claimants = survey.PropertyUnitId.HasValue
            ? await _relationRepository.GetPersonIdsWithActiveRelationsAsync(
                persons.Select(p => p.Id), survey.PropertyUnitId.Value, cancellationToken)
            : new HashSet<Guid>();

        foreach (var dto in persons)
        {
            dto.IsContactPerson = survey.ContactPersonId == dto.Id;
            dto.IsRightsClaimant = claimants.Contains(dto.Id);
        }
    }

    public async Task<List<DesignatedPersonDto>> GetPropertyUnitPersonsAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        var relations = (await _relationRepository.GetByPropertyUnitIdAsync(propertyUnitId, cancellationToken))
            .Where(r => r.IsActive && !r.IsDeleted)
            .ToList();
        var surveys = await _surveyRepository.GetByPropertyUnitAsync(propertyUnitId, cancellationToken);
        var contactIds = surveys
            .Where(s => s.ContactPersonId.HasValue)
            .Select(s => s.ContactPersonId!.Value)
            .ToHashSet();

        var personIds = relations.Select(r => r.PersonId).Concat(contactIds).Distinct().ToList();
        var persons = await _personRepository.GetByIdsAsync(personIds, cancellationToken);
        var relationsByPerson = relations
            .GroupBy(r => r.PersonId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return persons
            .Select(p => ToDto(p, contactIds.Contains(p.Id),
                relationsByPerson.GetValueOrDefault(p.Id) ?? new List<PersonPropertyRelation>()))
            .OrderByDescending(d => d.IsContactPoint)
            .ThenByDescending(d => d.IsRightsClaimant)
            .ThenBy(d => d.FullNameArabic)
            .ToList();
    }

    public async Task<DesignatedPersonDto?> GetContactPointAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        if (!survey.ContactPersonId.HasValue) return null;

        var person = await _personRepository.GetByIdAsync(survey.ContactPersonId.Value, cancellationToken);
        if (person == null) return null;

        var relations = new List<PersonPropertyRelation>();
        if (survey.PropertyUnitId.HasValue)
        {
            relations = (await _relationRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken))
                .Where(r => r.PersonId == person.Id && r.IsActive && !r.IsDeleted)
                .ToList();
        }

        return ToDto(person, isContactPoint: true, relations);
    }

    private static DesignatedPersonDto ToDto(Person person, bool isContactPoint, List<PersonPropertyRelation> relations)
    {
        // Primary relation: an ownership claim wins, then the most recent
        var relation = relations
            .OrderByDescending(r => ClaimTypeRelationMapper.ToClaimType(r.RelationType) == Domain.Enums.ClaimType.OwnershipClaim)
            .ThenByDescending(r => r.CreatedAtUtc)
            .FirstOrDefault();

        return new DesignatedPersonDto
        {
            PersonId = person.Id,
            FirstNameArabic = person.FirstNameArabic,
            FatherNameArabic = person.FatherNameArabic,
            FamilyNameArabic = person.FamilyNameArabic,
            MotherNameArabic = person.MotherNameArabic,
            FullNameArabic = person.GetFullNameArabic(),
            NationalId = person.NationalId,
            MobileNumber = person.MobileNumber,
            PhoneNumber = person.PhoneNumber,
            HouseholdId = person.HouseholdId,
            IsContactPoint = isContactPoint,
            IsRightsClaimant = relation != null,
            RelationType = relation != null ? (int)relation.RelationType : null,
            ClaimType = relation != null ? (int)ClaimTypeRelationMapper.ToClaimType(relation.RelationType) : null,
            RelationId = relation?.Id,
            Relations = relations.Select(r => new DesignatedRelationDto
            {
                RelationId = r.Id,
                RelationType = (int)r.RelationType,
                ClaimType = (int)ClaimTypeRelationMapper.ToClaimType(r.RelationType)
            }).ToList()
        };
    }
}
