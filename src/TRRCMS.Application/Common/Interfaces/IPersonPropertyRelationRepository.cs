using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for PersonPropertyRelation entity operations
/// </summary>
public interface IPersonPropertyRelationRepository
{
    Task<PersonPropertyRelation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PersonPropertyRelation?> GetByIdWithEvidencesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PersonPropertyRelation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PersonPropertyRelation>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PersonPropertyRelation>> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PersonPropertyRelation>> GetByPropertyUnitIdWithEvidencesAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PersonPropertyRelation>> GetBySurveyIdWithEvidencesAsync(Guid surveyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PersonPropertyRelation>> GetActiveRelationsByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
    Task<PersonPropertyRelation?> GetByPersonAndPropertyUnitAsync(Guid personId, Guid propertyUnitId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PersonPropertyRelation> AddAsync(PersonPropertyRelation relation, CancellationToken cancellationToken = default);
    Task UpdateAsync(PersonPropertyRelation relation, CancellationToken cancellationToken = default);
    Task DeleteAsync(PersonPropertyRelation relation, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
