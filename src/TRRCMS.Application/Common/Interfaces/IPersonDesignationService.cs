using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Computes the two person designations used across the system:
/// <list type="bullet">
/// <item><b>Contact Point</b> (جهة الاتصال) — the survey's contact person (<see cref="Survey.ContactPersonId"/>).</item>
/// <item><b>Rights Claimant</b> (صاحب حق) — a person with an active person–property relation (which becomes a claim).</item>
/// </list>
/// Both are derived from existing data so they can never drift from the relations/surveys they describe.
/// </summary>
public interface IPersonDesignationService
{
    /// <summary>
    /// Global scope: contact point of any survey (or legacy flag); claimant on any property.
    /// </summary>
    Task ApplyAsync(IReadOnlyCollection<PersonDto> persons, CancellationToken cancellationToken = default);

    /// <summary>
    /// Survey scope: contact point of this survey; claimant on the survey's property unit.
    /// </summary>
    Task ApplyForSurveyAsync(IReadOnlyCollection<PersonDto> persons, Survey survey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Everyone linked to a property unit — claimants plus the contact points of its surveys —
    /// contact points first.
    /// </summary>
    Task<List<DesignatedPersonDto>> GetPropertyUnitPersonsAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The contact point of a survey with its designations on the survey's unit, or null.
    /// </summary>
    Task<DesignatedPersonDto?> GetContactPointAsync(Survey survey, CancellationToken cancellationToken = default);
}
